﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ImageAPI
{
    public static class WebApplicationBuilderExtension
    {
        public static WebApplicationBuilder AddAppAuthetication(this WebApplicationBuilder builder, string jwtKey)
        {
            var settingsSection = builder.Configuration.GetSection("ApiSettings");

            var issuer = settingsSection.GetValue<string>("Issuer");
            var audience = settingsSection.GetValue<string>("Audience");

            var key = Encoding.ASCII.GetBytes(jwtKey);


            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ValidateAudience = true
                };
            });

            return builder;
        }
    }
}
