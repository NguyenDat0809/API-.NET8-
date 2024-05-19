using Data.Models;
using FinShark.Repositories;
using IdentityAPIDemo.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Services.Models;
using Services.Repositories.Implements;
using Services.Repositories.Interfaces;
using Services.Services.Implements;
using Services.Services.Interfaces;
using System.Text;
using System.Text.Json.Serialization;

namespace IdentityAPIDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //thêm service Newtonsoft.Json cho Controller
            //-> mục tiêu là ngăn chặn cycle n + 1 (vòng lặp vô hạn) khi truy xuất include
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            builder.Services.AddControllers().AddJsonOptions(x =>
            {
                //new JsonStringEnumConverter(): converter để chuyển đổi các giá trị enum thành chuỗi JSON và ngược lại
                //Mặc định, các giá trị enum sẽ được serialize thành số, nhưng với converter này, chúng sẽ được serialize thành chuỗi tương ứng.
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                //new TimeOnlyJsonConverter(): Thêm một converter tùy chỉnh (custom converter) để xử lý kiểu dữ liệu TimeOnly trong .NET 6, mà không có sẵn trong bộ serialize JSON mặc định.
                //x.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
            });

            //add JWTBearer authentication configuration for swagger
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string [] {}
                    }
                });
            });

            //add EF
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            //Add Identity config
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(10));

            //Add authentication JWTBearer
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])),
                    //tham số ClockSkew xử lý chênh lệch, thời gian giữa máy chủ và máy khách
                    /*
                        Mặc định, giá trị của ClockSkew là 5 phút. 
                        Điều này có nghĩa là ngay cả khi token đã hết hạn, nó vẫn có thể được coi là hợp lệ trong khoảng thời gian tối đa 5 phút sau thời điểm hết hạn, để giảm thiểu các vấn đề do sự chênh lệch đồng hồ giữa các máy chủ
                     */
                    ClockSkew = TimeSpan.Zero,
                };
                
            });

            //Add email config
            var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();


            builder.Services.AddSingleton(emailConfig);
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IUserManagement, UserManagement>();
            builder.Services.AddScoped<IStockRepository, StockRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IPortfolioRepository, Portfoliorepository>();

            //cấu hình serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                //còn nhiều thuộc tính khác nhưng dang tập trung vào basic
                .WriteTo.File("serilog.txt")
                .CreateLogger();
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerUI();
                app.UseSwagger();
            }
            //thêm middleware mới định nghĩa vào
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
