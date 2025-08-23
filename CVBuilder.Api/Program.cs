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

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDatabase(builder.Configuration);

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICVService, CVService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPdfService, PdfService>();
            builder.Services.AddScoped<ITemplateService, TemplateService>();
            builder.Services.AddScoped<TemplateSeeder>();


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

            using (var scope = app.Services.CreateScope())
            {
                var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                logger.LogInformation("Environment: {Env}", env.EnvironmentName);

                if (env.IsDevelopment() && cfg.GetValue<bool>("RunMigrationsAtStartup"))
                {
                    var pending = ctx.Database.GetPendingMigrations();
                    if (pending.Any())
                    {
                        logger.LogInformation("Applying {Count} pending migrations…", pending.Count());
                        ctx.Database.Migrate();
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations.");
                    }
                }

                if (cfg.GetValue<bool>("SeedTemplatesAtStartup"))
                {
                    var seeder = scope.ServiceProvider.GetRequiredService<TemplateSeeder>();
                    logger.LogInformation("Running TemplateSeeder…");
                    seeder.SeedAsync().GetAwaiter().GetResult();
                    logger.LogInformation("TemplateSeeder finished.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            } 

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("_myAllowSpecificOrigins");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
