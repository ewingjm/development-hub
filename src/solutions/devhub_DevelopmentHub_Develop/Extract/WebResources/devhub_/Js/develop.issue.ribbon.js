"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var DevelopmentHub;
(function (DevelopmentHub) {
    var Develop;
    (function (Develop) {
        var StartDevelopingRequest = /** @class */ (function () {
            function StartDevelopingRequest(entity) {
                this.entity = entity;
            }
            StartDevelopingRequest.prototype.getMetadata = function () {
                return {
                    boundParameter: "entity",
                    operationType: 0,
                    operationName: "devhub_StartDeveloping",
                    parameterTypes: {
                        entity: {
                            typeName: "mscrm.devhub_issue",
                            structuralProperty: 5
                        }
                    }
                };
            };
            return StartDevelopingRequest;
        }());
        function startDeveloping(primaryControl) {
            return __awaiter(this, void 0, void 0, function () {
                var result, ex_1, errorResponse;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            Xrm.Utility.showProgressIndicator("Creating development solution");
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, Xrm.WebApi.online.execute(new StartDevelopingRequest(primaryControl.data.entity.getEntityReference()))];
                        case 2:
                            result = _a.sent();
                            return [3 /*break*/, 4];
                        case 3:
                            ex_1 = _a.sent();
                            Xrm.Utility.closeProgressIndicator();
                            errorResponse = ex_1;
                            Xrm.Navigation.openErrorDialog({
                                message: errorResponse.message,
                                errorCode: errorResponse.errorCode,
                            });
                            return [3 /*break*/, 4];
                        case 4:
                            Xrm.Utility.closeProgressIndicator();
                            if (result.ok) {
                                primaryControl.data.refresh(false);
                            }
                            return [2 /*return*/];
                    }
                });
            });
        }
        Develop.startDeveloping = startDeveloping;
        function isStartDevelopingEnabled(primaryControl) {
            var statusCode = primaryControl.getAttribute("statuscode");
            return statusCode.getValue() === 1 /* ToDo */ && primaryControl.ui.getFormType() === 2 /* Update */;
        }
        Develop.isStartDevelopingEnabled = isStartDevelopingEnabled;
    })(Develop = DevelopmentHub.Develop || (DevelopmentHub.Develop = {}));
})(DevelopmentHub || (DevelopmentHub = {}));
//# sourceMappingURL=develop.issue.ribbon.js.map