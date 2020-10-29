// eslint-disable-next-line @typescript-eslint/no-unused-vars
namespace DevelopmentHub.Develop {
  class ApproveRequest {
    public entity: Xrm.LookupValue;

    constructor(entity: Xrm.LookupValue) {
      this.entity = entity;
    }

    // eslint-disable-next-line class-methods-use-this
    public getMetadata() {
      return {
        boundParameter: 'entity',
        operationType: 0,
        operationName: 'devhub_Approve',
        parameterTypes: {
          entity: {
            typeName: 'mscrm.devhub_solutionmerge',
            structuralProperty: 5,
          },
        },
      };
    }
  }

  class RejectRequest {
    public entity: Xrm.LookupValue;

    constructor(entity: Xrm.LookupValue) {
      this.entity = entity;
    }

    // eslint-disable-next-line class-methods-use-this
    public getMetadata() {
      return {
        boundParameter: 'entity',
        operationType: 0,
        operationName: 'devhub_Reject',
        parameterTypes: {
          entity: {
            typeName: 'mscrm.devhub_solutionmerge',
            structuralProperty: 5,
          },
        },
      };
    }
  }

  function handleActionResponse() {
    Xrm.Utility.closeProgressIndicator();
  }

  function handleActionError(err: { message: string, errorCode: number }) {
    Xrm.Utility.closeProgressIndicator();
    Xrm.Navigation.openErrorDialog({
      message: err.message,
      errorCode: err.errorCode,
    });
  }

  export function isReviewEnabled(primaryControl: Xrm.FormContext): boolean {
    return primaryControl.getAttribute('statuscode').getValue() === SolutionMergeStatusCode.AwaitingReview
            && primaryControl.ui.getFormType() !== XrmEnum.FormType.Create;
  }

  export function retry(primaryControl: Xrm.FormContext): void {
    primaryControl.getAttribute('statuscode').setValue(353400000);
    primaryControl.data.save();
  }

  function executeWebApiRequest(request: any, progressIndicator: string): Promise<void> {
    return new Promise((res, rej) => {
      Xrm.Utility.showProgressIndicator(progressIndicator);
      Xrm.WebApi.online.execute(request)
        .then(handleActionResponse, handleActionError)
        .then(res)
        .catch(rej);
    });
  }

  export function approve(primaryControl: Xrm.FormContext): void {
    const entity = primaryControl.data.entity.getEntityReference();

    executeWebApiRequest(new ApproveRequest(entity), 'Approving solution merge.')
      .then(async () => {
        // Flows don't trigger for updates done within actions/workflows.
        // Setting the statuscode here as a workaround
        primaryControl.getAttribute('statuscode').setValue(353400000);
        await primaryControl.data.save();
        primaryControl.data.refresh(false);
      });
  }

  export function reject(primaryControl: Xrm.FormContext): void {
    const entity = primaryControl.data.entity.getEntityReference();

    executeWebApiRequest(new RejectRequest(entity), 'Rejecting solution merge.')
      .then(() => primaryControl.data.refresh(false));
  }
}
