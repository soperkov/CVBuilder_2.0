global using CVBuilder.Core.Dtos;
global using CVBuilder.Core.Models;
global using CVBuilder.Core.Services;
global using CVBuilder.Core.Interfaces;
global using CVBuilder.Db.Contexts;
global using CVBuilder.Core.Settings;
global using CVBuilder.Db.Extensions;
global using CVBuilder.Api.Services;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components;

global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;