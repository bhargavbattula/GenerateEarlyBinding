using CodeWriter;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigLynx
{
    public class CrmClassGenerator
    {
        public void GenerateClassFile(EntityMetadataCollection entityMetaDataCollection)
        {
            var w = new CodeWriter.CodeWriter(CodeWriterSettings.CSharpDefault);

            w._("using System;");
            w._("using System.Collections.Generic;");
            w._("using System.Linq;");
            w._("using System.Text;");
            w._("using System.Threading.Tasks;");
            w._("using Microsoft.Xrm.Sdk;");
            w._("");

            using (w.B("namespace Xrm"))
            {
                foreach (EntityMetadata entityMetaData in entityMetaDataCollection)
                {

                    using (w.B("public class " + entityMetaData.LogicalName + ": Entity"))
                    {
                        var attributes = entityMetaData.Attributes.Where(filter => filter.AttributeType.ToString() != "Virtual").ToList();
                        foreach (var att in attributes)
                        {
                            w._("public " + GetAttributeTypeName(att.AttributeType.ToString()) + " " + att.LogicalName + "{ get; set;}");
                        }
                        using (w.B("public Entity ToEntity()"))
                        {
                            w._("Entity ent = new Entity(this.GetType().Name);");

                            using (w.B("try"))
                            {
                                using (w.B("foreach (var att in this.GetType().GetProperties())"))
                                {
                                    using (w.B("if (att.Name != \"KeyAttributes\" && att.Name != \"Item\" && att.Name  != \"ExtensionData\" && att.Name != \"entityimageid\" && att.Name != \"Attributes\" && att.Name != \"FormattedValues\" && att.Name != \"RelatedEntities\")"))
                                    {
                                        using (w.B("if (att.GetValue(this) != null)"))
                                        {
                                            using (w.B("if (att.GetValue(this).GetType().Name != \"Guid\")"))
                                            {
                                                w._("ent[att.Name] = att.GetValue(this);");
                                            }
                                            using (w.B("else if(att.GetValue(this).GetType().Name == \"Guid\" && (Guid)att.GetValue(this) != Guid.Empty)"))
                                            {
                                                w._("ent[att.Name] = att.GetValue(this);");
                                            }
                                        }
                                    }
                                }
                            }
                            using (w.B("catch (Exception ex)"))
                            {
                                w._("throw new Exception(ex.Message);");
                            }
                        }
                    }
                }
            }
            string executableLocation = Path.GetDirectoryName(
    System.Reflection.Assembly.GetExecutingAssembly().Location);
            string csLocation = Path.Combine(executableLocation, "Xrm.cs");
            File.WriteAllText(csLocation, w.ToString());
            Console.WriteLine(csLocation);
        }
        private string GetAttributeTypeName(string attributeType)
        {

            string returnString = string.Empty;
            switch (attributeType)
            {
                case "BigInt":
                    returnString = "Int64";
                    break;
                case "String":
                    returnString = "String";
                    break;
                case "DateTime":
                    returnString = "System.Nullable<DateTime>";
                    break;
                case "Double":
                    returnString = "System.Nullable<double>";
                    break;
                case "Decimal":
                    returnString = "System.Nullable<decimal>";
                    break;
                case "Boolean":
                    returnString = "System.Nullable<Boolean>";
                    break;
                case "Status":
                    returnString = "OptionSetValue";
                    break;
                case "State":
                    returnString = "OptionSetValue";
                    break;
                case "Integer":
                    returnString = "System.Nullable<int>";
                    break;
                case "Memo":
                    returnString = "String";
                    break;
                case "Lookup":
                    returnString = "EntityReference";
                    break;
                case "EntityName":
                    returnString = "String";
                    break;
                case "Customer":
                    returnString = "EntityReference";
                    break;
                case "Uniqueidentifier":
                    returnString = "Guid";
                    break;
                case "Money":
                    returnString = "Microsoft.Xrm.Sdk.Money";
                    break;
                case "Picklist":
                    returnString = "Microsoft.Xrm.Sdk.OptionSetValueCollection";
                    break;
                case "Owner":
                    returnString = "EntityReference";
                    break;
                case "iscustomizable":
                    returnString = "bool";
                    break;
            }
            return returnString;
        }
    }
}
