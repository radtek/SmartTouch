﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Community
{
    public interface ICommunityRepository
    {
        Community FindByCommunityName(string communityName);
    }
}
