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

  async function getActiveIssues(select: string[]) {
    return (await Xrm.WebApi.online.retrieveMultipleRecords(
      'devhub_issue',
      `?$select=${select.join(',')}&$filter=statuscode eq 353400000 or statuscode eq 353400002`,
    )).entities;
  }

  async function getConflictingSolutions(context: Xrm.FormContext): Promise<any[]> {
    const issueAttr: Xrm.Attributes.LookupAttribute = context.getAttribute('devhub_issue');

    const issueRef = issueAttr && issueAttr.getValue();
    if (!issueRef) {
      return [];
    }

    const activeIssues = await getActiveIssues(['devhub_developmentsolution', 'devhub_name']);
    if (!activeIssues || activeIssues.length === 0) {
      return [];
    }

    const thisIssue = activeIssues.find((i) => `{${i.devhub_issueid.toUpperCase()}}` === issueRef[0].id);
    const filter = `(Microsoft.Dynamics.CRM.In(PropertyName='uniquename',PropertyValues=[${activeIssues.map((i) => `'${i.devhub_developmentsolution}'`)}]))`;
    const solutions = (await Xrm.WebApi.online.retrieveMultipleRecords(
      'solution',
      `?$select=uniquename&$filter=${filter}&$expand=solution_solutioncomponent($select=objectid,componenttype,rootcomponentbehavior,rootsolutioncomponentid)`,
    )).entities;

    const thisSolutionIndex = solutions
      .findIndex((s) => s.uniquename === thisIssue.devhub_developmentsolution);
    const toMergeSol = solutions[thisSolutionIndex];
    solutions.splice(thisSolutionIndex, 1);

    const conflicts = solutions.filter((sol) => sol.solution_solutioncomponent.some((comp) => {
      const match = toMergeSol.solution_solutioncomponent.find(
        (toMergeComp) => comp.objectid === toMergeComp.objectid,
      );

      // Entity
      if (comp.componenttype === 1) {
        if (match && comp.rootcomponentbehavior < 2 && match.rootcomponentbehavior < 2) {
          // Check for both entities including all subcomponents or metadata
          return true;
        }
        if (comp.rootcomponentbehavior === 0) {
          // Check for to merge solution containing a subcomponent of this entity
          const entityComponent = toMergeSol.solution_solutioncomponent.find(
            (c) => c.objectid === comp.objectid,
          );
          return toMergeSol.solution_solutioncomponent.some(
            (toMergeComponent) => entityComponent.solutioncomponentid
              === toMergeComponent.rootsolutioncomponentid,
          );
        }
      }

      if (!match && comp.rootsolutioncomponentid) {
        // Check for to merge solution containing the root entity with all subcomponents
        return toMergeSol.solution_solutioncomponent.some(
          (toMergeComponent) => toMergeComponent.objectid === comp.rootsolutioncomponentid
            && toMergeComponent.rootcomponentbehavior === 0,
        );
      }

      return !!match;
    }));

    return conflicts;
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

  export async function approve(primaryControl: Xrm.FormContext) {
    const entity = primaryControl.data.entity.getEntityReference();

    Xrm.Utility.showProgressIndicator('Checking for conflicts.');
    const conflictingSolutions = await getConflictingSolutions(primaryControl);
    if (conflictingSolutions.length > 0) {
      const confirmResult = await Xrm.Navigation.openConfirmDialog({
        text: `Components in this solution were also found in the following solutions: ${conflictingSolutions.map((s) => s.uniquename).join(', ').toString()}. Please ensure that unintended changes have not been introduced changes to components in this solution.`,
        title: 'Possible conflict detected.',
        confirmButtonLabel: 'Approve',
      });

      if (!confirmResult.confirmed) {
        Xrm.Utility.closeProgressIndicator();
        return;
      }
    }
    await executeWebApiRequest(new ApproveRequest(entity), 'Approving solution merge.');
    await primaryControl.data.refresh(false);
    Xrm.Utility.closeProgressIndicator();
  }

  export function reject(primaryControl: Xrm.FormContext): void {
    const entity = primaryControl.data.entity.getEntityReference();

    executeWebApiRequest(new RejectRequest(entity), 'Rejecting solution merge.')
      .then(() => primaryControl.data.refresh(false));
  }
}
