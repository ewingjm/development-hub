var Capgemini;
(function (Capgemini) {
    var DevelopmentHub;
    (function (DevelopmentHub) {
        var Develop;
        (function (Develop) {
            var ApproveRequest = /** @class */ (function () {
                function ApproveRequest(entity) {
                    this.entity = entity;
                }
                ApproveRequest.prototype.getMetadata = function () {
                    return {
                        boundParameter: "entity",
                        operationType: 0,
                        operationName: "cap_Approve",
                        parameterTypes: {
                            entity: {
                                typeName: "mscrm.cap_solutionmerge",
                                structuralProperty: 5
                            }
                        }
                    };
                };
                return ApproveRequest;
            }());
            function isReviewEnabled(primaryControl) {
                return primaryControl.getAttribute("statuscode").getValue() === 1 /* AwaitingReview */
                    &&
                        primaryControl.ui.getFormType() !== 1 /* Create */;
            }
            Develop.isReviewEnabled = isReviewEnabled;
            function approve(primaryControl) {
                var entity = primaryControl.data.entity.getEntityReference();
                executeWebApiRequest(new ApproveRequest(entity), "Approving solution merge.")
                    .then(function () { return primaryControl.data.refresh(false); });
            }
            Develop.approve = approve;
            function executeWebApiRequest(request, progressIndicator) {
                return new Promise(function (resolve, reject) {
                    Xrm.Utility.showProgressIndicator(progressIndicator);
                    Xrm.WebApi.online.execute(request)
                        .then(handleActionResponse, handleActionError)
                        .then(resolve)
                        .catch(reject);
                });
            }
            function handleActionResponse(response) {
                Xrm.Utility.closeProgressIndicator();
            }
            function handleActionError(err) {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openErrorDialog({
                    message: err.message,
                    errorCode: err.errorCode
                });
            }
        })(Develop = DevelopmentHub.Develop || (DevelopmentHub.Develop = {}));
    })(DevelopmentHub = Capgemini.DevelopmentHub || (Capgemini.DevelopmentHub = {}));
})(Capgemini || (Capgemini = {}));
//# sourceMappingURL=develop.solutionmerge.ribbon.js.map