﻿namespace MockExams.Infra.Email
{
   public class EmailSettings
    {
        public string HostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string SenderName { get; set; }
    }
}
