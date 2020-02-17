"use strict";
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
                    operationName: "devhub_Approve",
                    parameterTypes: {
                        entity: {
                            typeName: "mscrm.devhub_solutionmerge",
                            structuralProperty: 5
                        }
                    }
                };
            };
            return ApproveRequest;
        }());
        var RejectRequest = /** @class */ (function () {
            function RejectRequest(entity) {
                this.entity = entity;
            }
            RejectRequest.prototype.getMetadata = function () {
                return {
                    boundParameter: "entity",
                    operationType: 0,
                    operationName: "devhub_Reject",
                    parameterTypes: {
                        entity: {
                            typeName: "mscrm.devhub_solutionmerge",
                            structuralProperty: 5
                        }
                    }
                };
            };
            return RejectRequest;
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
        function reject(primaryControl) {
            var entity = primaryControl.data.entity.getEntityReference();
            executeWebApiRequest(new RejectRequest(entity), "Rejecting solution merge.")
                .then(function () { return primaryControl.data.refresh(false); });
        }
        Develop.reject = reject;
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
})(DevelopmentHub || (DevelopmentHub = {}));
//# sourceMappingURL=develop.solutionmerge.ribbon.js.map