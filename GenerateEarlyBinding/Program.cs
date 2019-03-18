using BigLynx;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace GenerateEarlyBinding
{
    class Program
    {
        private static CrmServiceClient _client;
        private static EntityCollection solutionsList;
        private static int solutionNumber;
        private static Entity currentSolution;
        public static EntityCollection SolutionsList { get => solutionsList; set => solutionsList = value; }
        public static int SolutionNumber { get => solutionNumber; set => solutionNumber = value; }
        public static Entity CurrentSolution { get => currentSolution; set => currentSolution = value; }
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    string connectionString = "";
                    foreach (string str in args)
                        connectionString += str;
                    try
                    {
                        using (_client = new CrmServiceClient(connectionString))
                        {
                            Console.WriteLine("Connection Established Successfully...");
                            SolutionsList = new EntityCollection();
                            SolutionNumber = 0;
                            CurrentSolution = new Entity();
                            RetrieveCRMSolutions(_client);
                            var listOfEntitiesMetadata = RetrieveSolutionRelatedEntities(_client);
                            CrmClassGenerator crmClassGenerator = new CrmClassGenerator();
                            EntityMetadataCollection coll = new EntityMetadataCollection();
                            foreach (EntityMetadata meta in listOfEntitiesMetadata)
                                coll.Add(meta);
                            crmClassGenerator.GenerateClassFile(coll);

                            Console.WriteLine("File is located @ above location");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                string message = ex.Message;
                throw;
            }
        }
        private static IEnumerable<EntityMetadata> RetrieveSolutionRelatedEntities(IOrganizationService organizationService)
        {
            QueryExpression componentsQuery = new QueryExpression
            {
                EntityName = "solutioncomponent",
                ColumnSet = new ColumnSet("objectid"),
                Criteria = new FilterExpression(),
            };
            LinkEntity solutionLink = new LinkEntity("solutioncomponent", "solution", "solutionid", "solutionid", JoinOperator.Inner);
            solutionLink.LinkCriteria = new FilterExpression();
            solutionLink.LinkCriteria.AddCondition(new ConditionExpression("solutionid", ConditionOperator.Equal, CurrentSolution.Id));
            componentsQuery.LinkEntities.Add(solutionLink);
            componentsQuery.Criteria.AddCondition(new ConditionExpression("componenttype", ConditionOperator.Equal, 1));
            EntityCollection ComponentsResult = organizationService.RetrieveMultiple(componentsQuery);
            //Get all entities
            RetrieveAllEntitiesRequest AllEntitiesrequest = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity | Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };
            RetrieveAllEntitiesResponse AllEntitiesresponse = (RetrieveAllEntitiesResponse)organizationService.Execute(AllEntitiesrequest);
            return AllEntitiesresponse.EntityMetadata.Join(ComponentsResult.Entities.Select(x => x.Attributes["objectid"]), x => x.MetadataId, y => y, (x, y) => x);
        }

        private static void RetrieveCRMSolutions(IOrganizationService organizationService)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(true),
            };
            SolutionsList = organizationService.RetrieveMultiple(query);
            if (SolutionsList.Entities.Count > 0)
            {
                for (var i = 0; i < SolutionsList.Entities.Count; i++)
                {
                    Console.WriteLine(i + 1 + ". " + SolutionsList.Entities[i]["friendlyname"] + " (" + SolutionsList.Entities[i]["uniquename"] + ")");
                }
                Console.WriteLine("");
                Console.WriteLine("Please enter solution # and press 'Enter' key");
                var key = Console.ReadLine();
                int number;
                if (Int32.TryParse(key, out number))
                {
                    SolutionNumber = number;
                    if (SolutionsList.Entities.Count >= number - 1)
                        CurrentSolution = SolutionsList.Entities[number - 1];
                    else
                        Console.WriteLine("Enter a valid Number");
                }
                else
                {
                    Console.WriteLine("Enter a valid Number");
                }
            }
        }
    }
}