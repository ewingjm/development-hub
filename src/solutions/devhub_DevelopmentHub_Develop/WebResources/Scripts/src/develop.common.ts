// eslint-disable-next-line @typescript-eslint/no-unused-vars
namespace DevelopmentHub.Develop {
  class ExecuteWorkflowRequest {
    public entity: Xrm.LookupValue;

    public EntityId: { guid: string };

    constructor(workflow: Xrm.LookupValue, recordId: string) {
      this.entity = workflow;
      this.EntityId = {
        guid: recordId,
      };
    }

    // eslint-disable-next-line class-methods-use-this
    public getMetadata() {
      return {
        boundParameter: 'entity',
        operationType: 0,
        operationName: 'ExecuteWorkflow',
        parameterTypes: {
          entity: {
            typeName: 'mscrm.workflow',
            structuralProperty: 5,
          },
          EntityId: {
            typeName: 'Edm.Guid',
            structuralProperty: 1,
          },
        },
      };
    }
  }

  function onExecuteWorkflowResponse(result: Xrm.ExecuteResponse, primaryControl: Xrm.FormContext) {
    if (!result.ok) {
      return;
    }

    primaryControl.data
      .refresh(true)
      .then(
        primaryControl.ui.refreshRibbon,
        (err) => {
          Xrm.Navigation.openErrorDialog({
            message: err.message,
            errorCode: err.errorCode,
          });
        },
      );

    Xrm.Utility.closeProgressIndicator();
  }

  function onExecuteWorkflowError(error: Xrm.ErrorResponse) {
    Xrm.Utility.closeProgressIndicator();

    Xrm.Navigation.openErrorDialog({
      message: error.message,
      errorCode: error.errorCode,
    });
  }

  export function executeWorkflow(
    workflowId: string, recordId: string, text: string, primaryControl: Xrm.FormContext,
  ) {
    Xrm.Utility.showProgressIndicator(text);

    const workflow: Xrm.LookupValue = {
      entityType: 'workflow',
      id: workflowId,
    };

    Xrm.WebApi.online
      .execute(new ExecuteWorkflowRequest(workflow, recordId))
      .then(
        (result) => onExecuteWorkflowResponse(result, primaryControl),
        (error) => onExecuteWorkflowError(error),
      );
  }

  export function toggleFieldOnValue(
    context: Xrm.Events.EventContext,
    sourceField: string,
    value: any,
    targetField: string,
  ) {
    const formContext = context.getFormContext();

    const sourceAttribute = formContext.getAttribute(sourceField) as
      Xrm.Attributes.Attribute;
    if (!sourceAttribute) {
      return;
    }

    const targetControl = formContext.getControl(targetField) as
      Xrm.Controls.StandardControl;
    if (!targetControl) {
      return;
    }

    const sourceValue = sourceAttribute.getValue();
    const sourceType = sourceAttribute.getAttributeType();

    let toggle = false;
    switch (sourceType) {
      case 'multioptionset':
        toggle = sourceValue
          ? (JSON.stringify(sourceValue.sort()) === JSON.stringify(value.sort()))
          : sourceValue === value;
        break;
      case 'lookup':
        toggle = sourceValue ? sourceValue[0].id === value : sourceValue === value;
        break;
      default:
        toggle = sourceValue === value;
        break;
    }

    targetControl.setVisible(toggle);
    if (!toggle) {
      targetControl.getAttribute().setValue(null);
    }
  }
}
