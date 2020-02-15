namespace Capgemini.DevelopmentHub.Develop {
    class StartDevelopingRequest {
        public entity: Xrm.LookupValue;

        constructor(entity: Xrm.LookupValue) {
            this.entity = entity;
        }

        public getMetadata() {
            return {
                boundParameter: "entity",
                operationType: 0,
                operationName: "cap_StartDeveloping",
                parameterTypes: {
                    entity: {
                        typeName: "mscrm.cap_issue",
                        structuralProperty: 5
                    }
                }
            };
        }
    }

    export async function startDeveloping(primaryControl: Xrm.FormContext): Promise<void> {
        Xrm.Utility.showProgressIndicator("Creating development solution");
        let result: Xrm.ExecuteResponse;
        try {
            result = await Xrm.WebApi.online.execute(
                new StartDevelopingRequest(primaryControl.data.entity.getEntityReference()));
        } catch (ex) {
            Xrm.Utility.closeProgressIndicator();
            const errorResponse: Xrm.ErrorResponse = ex;
            Xrm.Navigation.openErrorDialog({
                message: errorResponse.message,
                errorCode: errorResponse.errorCode,
            })
        }
        
        Xrm.Utility.closeProgressIndicator();

        if (result.ok) {
            primaryControl.data.refresh(false);
        }
    }

    export function isStartDevelopingEnabled(primaryControl: Xrm.FormContext): boolean {
        const statusCode: Xrm.Attributes.OptionSetAttribute = primaryControl.getAttribute("statuscode");
        return statusCode.getValue() === cap_issue_statuscode.ToDo && primaryControl.ui.getFormType() === XrmEnum.FormType.Update;
    }
}