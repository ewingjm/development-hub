var Capgemini;
(function (Capgemini) {
    var DevelopmentHub;
    (function (DevelopmentHub) {
        var Develop;
        (function (Develop) {
            var ExecuteWorkflowRequest = /** @class */ (function () {
                function ExecuteWorkflowRequest(workflow, recordId) {
                    this.entity = workflow;
                    this.EntityId = {
                        guid: recordId
                    };
                }
                ExecuteWorkflowRequest.prototype.getMetadata = function () {
                    return {
                        boundParameter: "entity",
                        operationType: 0,
                        operationName: "ExecuteWorkflow",
                        parameterTypes: {
                            entity: {
                                typeName: "mscrm.workflow",
                                structuralProperty: 5
                            },
                            EntityId: {
                                typeName: "Edm.Guid",
                                structuralProperty: 1
                            }
                        }
                    };
                };
                return ExecuteWorkflowRequest;
            }());
            function executeWorkflow(workflowId, recordId, text, primaryControl) {
                Xrm.Utility.showProgressIndicator(text);
                var workflow = {
                    entityType: "workflow",
                    id: workflowId
                };
                Xrm.WebApi.online
                    .execute(new ExecuteWorkflowRequest(workflow, recordId))
                    .then(function (result) { return onExecuteWorkflowResponse(result, primaryControl); }, function (error) { return onExecuteWorkflowError(error); });
            }
            Develop.executeWorkflow = executeWorkflow;
            function onExecuteWorkflowResponse(result, primaryControl) {
                if (!result.ok) {
                    return;
                }
                primaryControl.data
                    .refresh(true)
                    .then(primaryControl.ui.refreshRibbon, function (err) {
                    Xrm.Navigation.openErrorDialog({
                        message: err.message,
                        errorCode: err.errorCode
                    });
                });
                Xrm.Utility.closeProgressIndicator();
            }
            function onExecuteWorkflowError(error) {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openErrorDialog({
                    message: error.message,
                    errorCode: error.errorCode
                });
            }
        })(Develop = DevelopmentHub.Develop || (DevelopmentHub.Develop = {}));
    })(DevelopmentHub = Capgemini.DevelopmentHub || (Capgemini.DevelopmentHub = {}));
})(Capgemini || (Capgemini = {}));
//# sourceMappingURL=develop.common.js.map