using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using Newtonsoft.Json.Linq;

/// <summary>
/// Deployment functionality for flows.
/// </summary>
public class FlowDeploymentService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlowDeploymentService"/> class.
    /// </summary>
    /// <param name="crmServiceClient">A service client authenticated as a licensed user.</param>
    /// <param name="packageLog">The logger.</param>
    public FlowDeploymentService(CrmServiceClient crmServiceClient, TraceLogger packageLog)
    {
        this.CrmSvc = crmServiceClient ?? throw new ArgumentNullException(nameof(crmServiceClient));
        this.PackageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
    }

    /// <summary>
    /// Gets a service client authenticated as a licensed user.
    /// </summary>
    protected CrmServiceClient CrmSvc { get; private set; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected TraceLogger PackageLog { get; private set; }

    /// <summary>
    /// Sets the connection on a flow.
    /// </summary>
    /// <param name="workflowId">The ID of the flow.</param>
    /// <param name="apiName">The API name (e.g. shared_sharepointonline).</param>
    /// <param name="connectionName">The connection name.</param>
    public void SetFlowConnection(Guid workflowId, string apiName, string connectionName)
    {
        if (string.IsNullOrEmpty(apiName))
        {
            throw new ArgumentException("You must provide an API name.", nameof(apiName));
        }

        if (string.IsNullOrEmpty(connectionName))
        {
            throw new ArgumentException("You must provide a connection name.", nameof(connectionName));
        }

        this.PackageLog.Log($"Setting connection name for {apiName} on flow {workflowId}.");

        var flow = this.CrmSvc.Retrieve("workflow", workflowId, new ColumnSet("clientdata"));
        flow["clientdata"] = this.GetClientDataWithConnectionName(flow.GetAttributeValue<string>("clientdata"), apiName, connectionName);
    }

    /// <summary>
    /// Activates a flow.
    /// </summary>
    /// <param name="flowId">The ID of the flow to activate.</param>
    public void ActivateFlow(Guid flowId)
    {
        this.PackageLog.Log($"Activating flow {flowId}.");

        if (!this.CrmSvc.UpdateStateAndStatusForEntity("workflow", flowId, 1, 2))
        {
            throw new InvalidOperationException($"Failed to activatate flow {flowId}.");
        }
    }

    /// <summary>
    /// Takes an array of workflow IDs and returns the flow IDs that were found on the target instance.
    /// </summary>
    /// <param name="guids">The flow IDs to find.</param>
    /// <param name="columnSet">The columns to select.</param>
    /// <returns>A collection of the flow IDs found on the target instance.</returns>
    public IEnumerable<Entity> GetDeployedFlows(IEnumerable<Guid> guids, ColumnSet columnSet)
    {
        if (guids is null)
        {
            throw new ArgumentNullException(nameof(guids));
        }

        if (!guids.Any())
        {
            throw new ArgumentException("You must provide at least one workflow ID.");
        }

        this.PackageLog.Log($"Getting deployed flows matching the following workflow IDs: {string.Join("\n - ", guids)}");
        var flowQuery = new QueryExpression("workflow")
        {
            ColumnSet = columnSet,
            Criteria = new FilterExpression(LogicalOperator.And),
        };
        flowQuery.Criteria.AddCondition("category", ConditionOperator.Equal, 5);
        flowQuery.Criteria.AddCondition("workflowid", ConditionOperator.In, guids.Cast<object>().ToArray());

        var results = this.CrmSvc.RetrieveMultiple(flowQuery);

        this.PackageLog.Log($"Found {results.Entities.Count} matching flows.");

        return results.Entities;
    }

    private string GetClientDataWithConnectionName(string clientData, string apiName, string connectionName)
    {
        var clientDataObject = JObject.Parse(clientData);
        var connectionReferences = (JObject)clientDataObject["properties"]["connectionReferences"];

        if (!connectionReferences.ContainsKey(apiName))
        {
            this.PackageLog.Log($"Unable to set connection name for {apiName}. No connections matching {apiName} were found in the flow.");
        }

        var connection = connectionReferences.Value<JObject>("connection");
        if (connection.ContainsKey("name") && connection["name"].ToString() != connectionName)
        {
            this.PackageLog.Log($"Updating existing connection name for {apiName}.");
            connection["name"] = connectionName;
        }
        else if (!connection.ContainsKey("name"))
        {
            this.PackageLog.Log($"Setting new connection name for {apiName}.");
            connection.Add("name", connectionName);
        }
        else
        {
            this.PackageLog.Log($"Connection name already set for {apiName}.");
        }

        return clientDataObject.ToString();
    }
}