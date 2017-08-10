﻿using System;

namespace ArchiveApi
{
    public class ArchiveException : Exception
    {
        public ArchiveException()
        {
        }

        public ArchiveException(string message) : base(message)
        {
        }

        public ArchiveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
