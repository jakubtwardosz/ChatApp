﻿using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Core.Domain.Interfaces.Services
{
    public interface IJwtService
    {
        AuthDto GenerateJwtToken(User user);
    }
}