﻿// <auto-generated />
namespace OutageDatabase.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.4.0")]
    public sealed partial class MergeActiveOutageandArchivedOutageintoOutageEntity : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(MergeActiveOutageandArchivedOutageintoOutageEntity));
        
        string IMigrationMetadata.Id
        {
            get { return "202003061017231_Merge ActiveOutage and ArchivedOutage into OutageEntity"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
