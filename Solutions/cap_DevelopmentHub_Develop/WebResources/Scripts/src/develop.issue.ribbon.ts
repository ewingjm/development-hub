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
        let result: Xrm.ExecuteResponse;
        try {
            result = await Xrm.WebApi.online.execute(
                new StartDevelopingRequest(primaryControl.data.entity.getEntityReference()));
        } catch (ex) {
            const errorResponse: Xrm.ErrorResponse = ex;
            Xrm.Navigation.openErrorDialog({
                message: errorResponse.message,
                errorCode: errorResponse.errorCode,
            })
        }

        if (result.ok) {
            await Xrm.Navigation.openAlertDialog({
                text: "Development solution created."
            });

            Xrm.Page.data.refresh(false);
        }
    }

    export function isStartDevelopingEnabled(primaryControl: Xrm.FormContext): boolean {
        const statusCode: Xrm.Attributes.OptionSetAttribute = primaryControl.getAttribute("statuscode");
        return statusCode.getValue() === cap_issue_statuscode.ToDo && primaryControl.ui.getFormType() === XrmEnum.FormType.Update;
    }
}