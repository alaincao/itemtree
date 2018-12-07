(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){

},{}],2:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("./src/Views/common");
var itemlist = require("./src/Views/ItemTTT/List");
var itemedit = require("./src/Views/ItemTTT/Edit");
///////
// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var ttt = {
    common: common,
    itemttt: {
        list: itemlist,
        edit: itemedit,
    },
};
window.ttt = ttt;

},{"./src/Views/ItemTTT/Edit":7,"./src/Views/ItemTTT/List":9,"./src/Views/common":10}],3:[function(require,module,exports){
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("../Views/common");
var BaseAutoItem_1 = require("../Utils/BaseAutoItem");
var ItemKO = /** @class */ (function (_super) {
    __extends(ItemKO, _super);
    function ItemKO($container, src) {
        var _this = _super.call(this, $container, src) || this;
        var self = _this;
        common.utils.ensureInteger({ observable: self.price, canBeZero: false, mustBePositive: true });
        return _this;
    }
    ItemKO.prototype.toDictObj = function (dict) {
        var rc = _super.prototype.toDictObj.call(this, dict);
        return rc;
    };
    return ItemKO;
}(BaseAutoItem_1.BaseAutoItem));
exports.ItemKO = ItemKO;

},{"../Utils/BaseAutoItem":6,"../Views/common":10}],4:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
var _a;
var common = require("./Views/common");
exports.Languages = (_a = common.utils.strEnum(['en', 'fr', 'nl']), _a.e), exports.allLanguages = _a.a;

},{"./Views/common":10}],5:[function(require,module,exports){
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
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("../Views/common");
function getUrlCode(originalCode) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    if (common.utils.stringIsNullOrWhitespace(originalCode))
                        return [2 /*return*/, null];
                    return [4 /*yield*/, common.url.getRequest(common.routes.api.getUrlCode, { originalCode: originalCode })];
                case 1: return [2 /*return*/, _a.sent()];
            }
        });
    });
}
exports.getUrlCode = getUrlCode;
function details(code) {
    return __awaiter(this, void 0, void 0, function () {
        var url, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.itemDetails.replace(common.routes.itemCodeParameter, code);
                    return [4 /*yield*/, common.url.postRequest(url, {})];
                case 1:
                    rc = _a.sent();
                    rc.item = rc.result;
                    delete rc.result;
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.details = details;
function save(p) {
    return __awaiter(this, void 0, void 0, function () {
        var rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, common.url.postRequest(common.routes.api.itemSave, p)];
                case 1:
                    rc = _a.sent();
                    rc.newCode = rc.result;
                    delete rc.result;
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.save = save;

},{"../Views/common":10}],6:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
var fieldTagAttribute = 'ttt-name';
var BaseAutoItem = /** @class */ (function () {
    function BaseAutoItem($container, src) {
        this.fieldNames = [];
        this.knockoutify($container, src);
    }
    BaseAutoItem.prototype.toDictObj = function (dict) {
        var self = this;
        var dictKO = self;
        if (dict == null)
            dict = {};
        $.each(self.fieldNames, function (i, fieldName) {
            var observable = dictKO[fieldName];
            dict[fieldName] = observable();
        });
        return dict;
    };
    BaseAutoItem.prototype.fromDictObj = function (dict) {
        var self = this;
        var dictKO = self;
        $.each(self.fieldNames, function (i, fieldName) {
            var observable = dictKO[fieldName];
            observable(dict[fieldName]);
        });
    };
    BaseAutoItem.prototype.knockoutify = function ($container, src) {
        var self = this;
        var dict = this;
        // For each tagged elements we find in '$container' ...
        $container.find("[" + fieldTagAttribute + "]").each(function (i, e) {
            var $e = $(e);
            var fieldName = $e.attr(fieldTagAttribute);
            // ... create a KO observable on 'this' and set it's value using 'src' ...
            dict[fieldName] = ko.observable(src[fieldName]);
            // ... and register this field to 'fieldNames'
            self.fieldNames.push(fieldName);
        });
    };
    return BaseAutoItem;
}());
exports.BaseAutoItem = BaseAutoItem;

},{}],7:[function(require,module,exports){
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
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("../common");
var dto = require("../../DTOs/Item");
var ctrl = require("../../Services/ItemController");
var message_saveSuccess = 'Item saved successfully';
var message_refreshFailed = 'An error occured while refresing the data: ';
var $blockingDiv;
var originalCode;
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            common.utils.log('edit.init() START', { p: p });
            $blockingDiv = p.$blockingDiv;
            common.utils.log('edit.init(): Create KO item');
            exports.item = new dto.ItemKO(p.$fieldsContainer, p.model);
            originalCode = exports.item.code();
            common.utils.log('edit.init(): Apply KO item');
            ko.applyBindings(exports.item, p.$fieldsContainer[0]);
            common.utils.log('edit.init(): Bind JQuery events');
            p.$btnSave.click(save);
            common.utils.log('edit.init() END');
            return [2 /*return*/];
        });
    });
}
exports.init = init;
function save() {
    return __awaiter(this, void 0, void 0, function () {
        var obj, rc, rcr;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.save(): START');
                    obj = exports.item.toDictObj();
                    common.utils.log('edit.save(): Launch save');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, ctrl.save({ originalCode: originalCode, item: obj })];
                case 1:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    common.utils.log('edit.save()', { rc: rc });
                    if (!!rc.success) return [3 /*break*/, 2];
                    common.html.showMessage(rc.errorMessage);
                    return [3 /*break*/, 4];
                case 2: return [4 /*yield*/, refresh(rc.newCode)];
                case 3:
                    rcr = _a.sent();
                    if (rcr)
                        common.html.showMessage(message_saveSuccess);
                    _a.label = 4;
                case 4:
                    common.utils.log('edit.save(): END');
                    return [2 /*return*/];
            }
        });
    });
}
function refresh(code) {
    return __awaiter(this, void 0, void 0, function () {
        var newUrl, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.refresh(): START');
                    newUrl = null;
                    if (code == null) {
                        code = originalCode;
                    }
                    else {
                        if (originalCode != code)
                            // URL must change!
                            newUrl = common.routes.itemTTT.itemEdit.replace(common.routes.itemCodeParameter, code);
                        originalCode = code;
                    }
                    common.utils.log('edit.refresh(): Launch request');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, ctrl.details(code)];
                case 1:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    common.utils.log('edit.refresh()', { rc: rc });
                    if (!rc.success) {
                        common.html.showMessage(message_refreshFailed + rc.errorMessage);
                        return [2 /*return*/, false];
                    }
                    common.utils.log('edit.refresh(): Load new item');
                    exports.item.fromDictObj(rc.item);
                    if (newUrl != null) {
                        common.utils.log("edit.refresh(): Update URL to \"" + newUrl + "\"");
                        common.url.pushHistory({ pathname: newUrl, newTitle: exports.item.name() });
                    }
                    common.utils.log('edit.refresh(): END');
                    return [2 /*return*/, true];
            }
        });
    });
}

},{"../../DTOs/Item":3,"../../Services/ItemController":5,"../common":10}],8:[function(require,module,exports){
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
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("../common");
var list;
(function (list) {
    var _a, _b;
    _a = common.utils.strEnum(['grid', 'list']), list.ViewModes = _a.e, list.allViewModes = _a.a;
    _b = common.utils.strEnum(['price', 'price_desc', 'name', 'name_desc']), list.SortingFields = _b.e, list.allSortingFields = _b.a;
    function getListContent(p) {
        return __awaiter(this, void 0, void 0, function () {
            var url;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        url = common.routes.itemTTT.itemsList.replace(common.routes.languageParameter, common.pageParameters.currentLanguage);
                        return [4 /*yield*/, common.url.getRequest(url, p)];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    }
    list.getListContent = getListContent;
    ;
})(list = exports.list || (exports.list = {}));

},{"../common":10}],9:[function(require,module,exports){
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
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("../common");
var ItemTTTController_1 = require("./ItemTTTController");
var currentSortingField;
var $divCarsList;
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            common.utils.log('list.init() START');
            currentSortingField = p.sortingField;
            $divCarsList = p.$divCarsList;
            p.$btnViewGrid.click(function () { refreshList({ viewMode: ItemTTTController_1.list.ViewModes.grid }); });
            p.$btnViewList.click(function () { refreshList({ viewMode: ItemTTTController_1.list.ViewModes.list }); });
            p.$btnSortName.click(function () { refreshList({ sortingField: ItemTTTController_1.list.SortingFields.name }); });
            p.$btnSortPrice.click(function () { refreshList({ sortingField: ItemTTTController_1.list.SortingFields.price }); });
            common.utils.log('list.init() END');
            return [2 /*return*/];
        });
    });
}
exports.init = init;
function refreshList(p) {
    return __awaiter(this, void 0, void 0, function () {
        var html;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('list.refreshList() START', { p: p, currentSortingField: currentSortingField });
                    // Ask for the stripped version of the page (no layout)
                    p.noLayout = true;
                    // Switch to descending order if required
                    if (p.sortingField != null) {
                        if (p.sortingField == currentSortingField) {
                            switch (currentSortingField) {
                                case ItemTTTController_1.list.SortingFields.name:
                                    p.sortingField = ItemTTTController_1.list.SortingFields.name_desc;
                                    break;
                                case ItemTTTController_1.list.SortingFields.price:
                                    p.sortingField = ItemTTTController_1.list.SortingFields.price_desc;
                                    break;
                            }
                        }
                        currentSortingField = p.sortingField;
                    }
                    return [4 /*yield*/, ItemTTTController_1.list.getListContent(p)];
                case 1:
                    html = _a.sent();
                    $divCarsList.html(html);
                    common.utils.log('list.refresh() END', { currentSortingField: currentSortingField });
                    return [2 /*return*/];
            }
        });
    });
}

},{"../common":10,"./ItemTTTController":8}],10:[function(require,module,exports){
function __export(m) {
    for (var p in m) if (!exports.hasOwnProperty(p)) exports[p] = m[p];
}
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("./common");
exports.debugMessages = false; // NB: 'export' so that it can be easily changed from the browser's console
function init(p) {
    exports.pageParameters = p.pageParameters;
    exports.routes = exports.pageParameters.routes;
    utils.log('common.init()');
}
exports.init = init;
var utils;
(function (utils) {
    function log() {
        var optionalParams = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            optionalParams[_i] = arguments[_i];
        }
        if (exports.pageParameters.isDebug)
            console.log.apply(console, arguments);
    }
    utils.log = log;
    function error() {
        var optionalParams = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            optionalParams[_i] = arguments[_i];
        }
        console.error.apply(console, arguments);
    }
    utils.error = error;
    /** Function to simulate string-valued enums
     * Based on: https://basarat.gitbooks.io/typescript/docs/types/literal-types.html
     * Returns: { e:the enum , a:the array specified as parameter } */
    function strEnum(a) {
        var e = a.reduce(function (res, key) {
            res[key] = key;
            return res;
        }, Object.create(null));
        return { e: e, a: a };
    }
    utils.strEnum = strEnum;
    function stringIsNullOrEmpty(str) {
        if (str == null)
            return true;
        if (str == '')
            return true;
        return false;
    }
    utils.stringIsNullOrEmpty = stringIsNullOrEmpty;
    function stringIsNullOrWhitespace(str) {
        if (str == null)
            return true;
        if (str.trim() == '')
            return true;
        return false;
    }
    utils.stringIsNullOrWhitespace = stringIsNullOrWhitespace;
    function htmlEncode(txt) {
        // TODO: Find a better way ?
        if (Array.isArray(txt))
            return $.map(txt, function (v) { return $(document.createElement('span')).text(v).html(); }).join('<br/>');
        else
            return $(document.createElement('span')).text(txt).html();
    }
    utils.htmlEncode = htmlEncode;
    function arrayContains(a, item) {
        return (a.indexOf(item) >= 0);
    }
    utils.arrayContains = arrayContains;
    //ES5 incompatible ...
    //	export function arrayUnique<T>(a:T[]) : T[]
    //	{
    //		//return a.filter( (v,i)=>( a.indexOf(v) === i ) );
    //		return [ ... new Set(a) ];
    //	}
    function arrayRemoveItem(a, item) {
        var i = a.indexOf(item);
        if (i < 0)
            return false;
        a.splice(i, 1);
        return true;
    }
    utils.arrayRemoveItem = arrayRemoveItem;
    function arraySum(a, f) {
        var rc = 0;
        a.forEach(function (e) {
            rc += f(e);
        });
        return rc;
    }
    utils.arraySum = arraySum;
    function arrayMoveItem(p) {
        // NB: Does not yet work when n>1 ...
        var n = 1;
        if (p.direction != null)
            n = (p.direction == 'up') ? -n : n;
        var i = p.list.indexOf(p.item); // Start position
        if (i < 0)
            throw { error: 'Could not find item in list', item: p.item };
        var j = i + n; // End position
        if (j < 0)
            j = 0;
        else if (j >= p.list.length)
            j = p.list.length - 1;
        if (i == j)
            // NOOP
            return p.list;
        // Swap items here
        var tmp = p.list[i];
        p.list[i] = p.list[j];
        p.list[j] = tmp;
        return p.list;
    }
    utils.arrayMoveItem = arrayMoveItem;
    function ensureInteger(p) {
        if (p.canBeZero == null)
            p.canBeZero = true;
        if (p.fallbackValue == null) {
            if (p.canBeZero)
                p.fallbackValue = 0;
            else
                p.fallbackValue = 1;
        }
        p.observable.subscribe(function (value) {
            var newValue = parseInt(value);
            if (isNaN(newValue))
                newValue = p.fallbackValue;
            else if ((!p.canBeZero) && (value == 0))
                newValue = p.fallbackValue;
            else if ((p.mustBePositive) && (value < 0))
                newValue = p.fallbackValue;
            if (value !== newValue)
                // Value must be changed
                p.observable(newValue);
        });
    }
    utils.ensureInteger = ensureInteger;
    /** Generates a GUID-like string (something that looks like one, but NOT a real one!!!)
     * cf. http://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid-in-javascript */
    function newGuid() {
        var S4 = function () {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
        };
        return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
    }
    utils.newGuid = newGuid;
    function sleep(ms) {
        return new Promise(function (resolve) { return setTimeout(resolve, ms); });
    }
    utils.sleep = sleep;
})(utils = exports.utils || (exports.utils = {})); // namespace utils
var html;
(function (html) {
    function showMessage(msg) {
        var $div = $('<div/>').text(msg);
        $('body').append($div);
        $div.dialog();
    }
    html.showMessage = showMessage;
    /** Invoke jQuery.blockUI's '.block()' on the specified element but supports multiple invokation on the same element */
    function block($e) {
        // Insert/increment a block counter as jQuery 'data()'
        var blockCounter = ($e.data('common_blockCounter') | 0) + 1;
        $e.data('common_blockCounter', blockCounter);
        if (blockCounter == 1)
            // This element is not blocked yet
            $e.block(); // TODO: ACA: jQuery.blockUI typings ...
        return $e;
    }
    html.block = block;
    /** Invoke jQuery.blockUI's '.unblock()' on the specified element except if it has been block()ed more than once */
    function unblock($e) {
        // Decrement the block counter in the jQuery 'data()'
        var blockCounter = ($e.data('common_blockCounter') | 0) - 1;
        $e.data('common_blockCounter', blockCounter);
        if (blockCounter < 0) {
            // There is a logic error somewhere...
            common.utils.error('INTERNAL ERROR: Unblock count > block count:', blockCounter);
            // Reset counter
            blockCounter = 0;
            $e.data('common_blockCounter', 0);
        }
        if (blockCounter == 0)
            // This element is no more blocked by anything else
            $e.unblock(); // TODO: ACA: jQuery.blockUI typings ...
        return $e;
    }
    html.unblock = unblock;
    function ensureVisible($e) {
        // Scroll
        var offset = $e.offset().top - (20 + $e.height());
        $('html, body').animate({ scrollTop: offset }); // cf.: https://stackoverflow.com/questions/4884839/how-do-i-get-an-element-to-scroll-into-view-using-jquery?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        // Blink
        $e.effect("highlight", {}, 2000); // cf.: https://stackoverflow.com/questions/5205445/jquery-blinking-highlight-effect-on-div?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
    }
    html.ensureVisible = ensureVisible;
})(html = exports.html || (exports.html = {})); // namespace html
var url;
(function (url_1) {
    var onChangedEvent = 'onUrlChanged';
    var onChangedCallbacks = $({});
    var onChangedCallbacksRegistered = false;
    function redirect(url) {
        window.location.href = url;
    }
    url_1.redirect = redirect;
    /** Parse a parameters string like 'foo=bar&hello=world' and return the a dictionary like {foo:'bar',hello:'world'} */
    function parseParameters(searchString) {
        if (searchString == null)
            searchString = window.location.search.substring(1);
        if (searchString.length == 0)
            return {};
        var tokens = searchString.split('&');
        var dict = {};
        for (var i = 0; i < tokens.length; ++i) {
            var pairs = tokens[i].split('=');
            var key = decodeURIComponent(pairs[0]);
            var value = decodeURIComponent(pairs[1]);
            // Try parse JSON
            try {
                value = JSON.parse(value);
            }
            catch (ex) { /* Not JSON => Keep the string as-is*/ }
            dict[key] = value;
        }
        return dict;
    }
    url_1.parseParameters = parseParameters;
    /** Transform a dictionary like {foo:'bar',hello:'world'} to a parameters string like 'foo=bar&hello=world' */
    function stringifyParameters(parms) {
        var pairs = [];
        $.each(parms, function (key, value) {
            key = encodeURIComponent(key);
            if ((value == null) || (typeof (value) === 'string') || (typeof (value) === 'number') || (typeof (value) === 'boolean')) { /*Keep as-is*/ }
            else
                // Convert to JSON
                value = JSON.stringify(value);
            value = encodeURIComponent(value);
            pairs.push(key + "=" + value);
        });
        return pairs.join('&');
    }
    url_1.stringifyParameters = stringifyParameters;
    /** Registers a callback to be invoked whenever the browser's URL changes (cf. 'pushHistory()') */
    function onChanged(callback) {
        // NB: The callback gets invoked ALSO when 'pushHistory()' is invoked
        if (onChangedCallbacksRegistered == false) {
            $(window).bind('popstate', function (evt) {
                onChangedCallbacks.trigger(onChangedEvent);
            });
            onChangedCallbacksRegistered = true;
        }
        onChangedCallbacks.bind(onChangedEvent, callback);
    }
    url_1.onChanged = onChanged;
    function createUrl(p) {
        var newPath = p.pathname;
        if (newPath == null)
            newPath = window.location.pathname;
        if (p.parameters != null) {
            var queryString = stringifyParameters(p.parameters);
            if (queryString.length > 0)
                newPath = newPath + '?' + queryString;
        }
        return newPath;
    }
    url_1.createUrl = createUrl;
    /** Change the browser's current URL without triggering an HTTP request (NB: Will trigger any registered 'onChanged()' callbacks) */
    function pushHistory(p) {
        var newPath = createUrl(p);
        window.history.pushState({}, p.newTitle, newPath);
        // Invoke any registered callbacks
        onChangedCallbacks.trigger(onChangedEvent);
    }
    url_1.pushHistory = pushHistory;
    // nb: ES5 incompatible ; requires "Promise" library
    function postRequest(url, request) {
        var requestStr = JSON.stringify(request);
        return new Promise(function (resolve, reject) {
            $.ajax({ type: 'POST',
                url: url,
                contentType: 'application/json',
                data: requestStr,
                dataType: 'json',
                success: function (data, textStatus, jqXHR) { return resolve(data); },
                error: function (jqXHR, textStatus, errorThrown) {
                    reject(textStatus);
                }
            });
        });
    }
    url_1.postRequest = postRequest;
    // nb: ES5 incompatible ; requires "Promise" library
    function getRequest(url, request) {
        if (request != null) {
            var parms = stringifyParameters(request);
            url = url + "?" + parms;
        }
        return new Promise(function (resolve, reject) {
            $.ajax({ type: 'GET',
                url: url,
                contentType: 'text/html',
                success: function (data, textStatus, jqXHR) { return resolve(data); },
                error: function (jqXHR, textStatus, errorThrown) {
                    reject(textStatus);
                }
            });
        });
    }
    url_1.getRequest = getRequest;
    function downloadFile(url) {
        $(document.body).append($("<iframe/>").attr({ src: url,
            style: 'visibility:hidden;display:none' }));
    }
    url_1.downloadFile = downloadFile;
})(url = exports.url || (exports.url = {})); // namespace url
// nb: Exports at the end or the order of execution breaks everything (i.e. strEnum must be defined before) ...
__export(require("../Language"));

},{"../Language":4,"./common":10}]},{},[1,2])

//# sourceMappingURL=site.js.map
