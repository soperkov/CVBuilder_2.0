using System.Text.Json.Serialization;

namespace CVBuilder.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:58668")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            builder.Services.AddControllers().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDatabase(builder.Configuration);

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICVService, CVService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ILanguageService, LanguageService>();
            builder.Services.AddScoped<IUploadsService, UploadsService>();

            builder.Services.AddSingleton<ITemplateCatalog>(sp =>
            {
                var asm = typeof(CVBuilder.Api.Templates.CvClassic).Assembly;
                return new TemplateCatalog(asm, "CVBuilder.Api.Templates");
            });
            builder.Services.AddScoped<ITemplateRenderService, TemplateRenderService>();
            builder.Services.AddScoped<IPlaywrightPdfService, PlaywrightPdfService>();
            builder.Services.AddScoped<PdfGenerator>();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IDownloadTicketService, DownloadTicketService>();

            builder.Services.AddScoped<JwtService>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped<EmailService>();

            builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            var app = builder.Build();

            //Microsoft.Playwright.Program.Main(["install"]);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            if (builder.Configuration.GetValue<bool>("SeedTemplatesAtStartup"))
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var catalog = scope.ServiceProvider.GetRequiredService<ITemplateCatalog>();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                                   .CreateLogger("TemplateSeeder");

                TemplateSeeder.SeedTemplatesAsync(db, catalog, logger)
                                  .GetAwaiter()
                                  .GetResult();
            }

            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                                  .CreateLogger("LanguageSeeder");
                try
                {
                    LanguageSeeder
                        .SeedLanguagesAsync(app.Services, logger)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while seeding languages.");
                }
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("_myAllowSpecificOrigins");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseSwagger();

            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "CVBuilder.Api v1");
            });

            app.MapControllers();

            app.Run();
        }
    }
}
