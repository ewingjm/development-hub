namespace Capgemini.DevelopmentHub.Develop {
    class ExecuteWorkflowRequest {
        public entity: Xrm.LookupValue;
        public EntityId: { guid: string };

        constructor(workflow: Xrm.LookupValue, recordId: string) {
            this.entity = workflow;
            this.EntityId = {
                guid: recordId
            };
        }

        public getMetadata() {
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
        }
    }

    export function executeWorkflow(workflowId: string, recordId: string, text: string, primaryControl: Xrm.FormContext) {
        Xrm.Utility.showProgressIndicator(text);

        const workflow: Xrm.LookupValue = {
            entityType: "workflow",
            id: workflowId
        };

        Xrm.WebApi.online
            .execute(new ExecuteWorkflowRequest(workflow, recordId))
            .then(
                result => onExecuteWorkflowResponse(result, primaryControl),
                error => onExecuteWorkflowError(error));
    }

    function onExecuteWorkflowResponse(result: Xrm.ExecuteResponse, primaryControl: Xrm.FormContext) {
        if (!result.ok) {
            return;
        }

        primaryControl.data
            .refresh(true)
            .then(
                primaryControl.ui.refreshRibbon,
                err => {
                    Xrm.Navigation.openErrorDialog({
                        message: err.message,
                        errorCode: err.errorCode
                    });
                });

        Xrm.Utility.closeProgressIndicator();
    }

    function onExecuteWorkflowError(error: Xrm.ErrorResponse) {
        Xrm.Utility.closeProgressIndicator();

        Xrm.Navigation.openErrorDialog({
            message: error.message,
            errorCode: error.errorCode
        });
    }
}