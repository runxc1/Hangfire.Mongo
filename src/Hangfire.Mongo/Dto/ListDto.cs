﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Hangfire.Mongo.Dto
{
#pragma warning disable 1591
    public class ListDto
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public DateTime? ExpireAt { get; set; }
    }
#pragma warning restore 1591
}