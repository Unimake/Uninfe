using System;
using Microsoft.EntityFrameworkCore;
using UniNFe.Database.Models;

namespace UniNFe.Database
{
    public class UniNFeContext : DbContext
    {
        public DbSet<Configuration> Configurations { get; set; }

        public string DbPath { get; private set; }

        public UniNFeContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}UniNFe.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}