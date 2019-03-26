namespace Jb.Test.DAL.Impl.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Start : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NugetUser",
                c => new
                    {
                        ApiKey = c.String(nullable: false, maxLength: 400),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ApiKey)
                .Index(t => t.ApiKey, unique: true);
            
            CreateTable(
                "dbo.PackageIds",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 800),
                        LastUpdate = c.DateTime(nullable: false),
                        NugetUser_ApiKey = c.String(nullable: false, maxLength: 400),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NugetUser", t => t.NugetUser_ApiKey, cascadeDelete: true)
                .Index(t => t.Id, unique: true)
                .Index(t => t.NugetUser_ApiKey);
            
            CreateTable(
                "dbo.PackageVersions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 800),
                        Version = c.String(nullable: false, maxLength: 800),
                        Title = c.String(),
                        Description = c.String(),
                        Published = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        IsLatestVersion = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.Id, t.Version })
                .ForeignKey("dbo.PackageIds", t => t.Id, cascadeDelete: true)
                .Index(t => new { t.Id, t.Version }, unique: true);
            
            CreateTable(
                "dbo.PackageVersionDatas",
                c => new
                    {
                        PackageVersionId = c.String(nullable: false, maxLength: 1600),
                        Data = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.PackageVersionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PackageVersions", "Id", "dbo.PackageIds");
            DropForeignKey("dbo.PackageIds", "NugetUser_ApiKey", "dbo.NugetUser");
            DropIndex("dbo.PackageVersions", new[] { "Id", "Version" });
            DropIndex("dbo.PackageIds", new[] { "NugetUser_ApiKey" });
            DropIndex("dbo.PackageIds", new[] { "Id" });
            DropIndex("dbo.NugetUser", new[] { "ApiKey" });
            DropTable("dbo.PackageVersionDatas");
            DropTable("dbo.PackageVersions");
            DropTable("dbo.PackageIds");
            DropTable("dbo.NugetUser");
        }
    }
}
