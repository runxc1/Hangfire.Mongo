﻿using MongoDB.Bson;
using System;

namespace Hangfire.Mongo.Dto
{
#pragma warning disable 1591
    public class JobDetailedDto
    {
        public int Id { get; set; }

        public string InvocationData { get; set; }

        public string Arguments { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpireAt { get; set; }

        public DateTime? FetchedAt { get; set; }

        public ObjectId StateId { get; set; }

        public string StateName { get; set; }

        public string StateReason { get; set; }

        public string StateData { get; set; }
    }
#pragma warning restore 1591
}