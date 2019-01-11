(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){

},{}],2:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("./src/Views/common");
var adminhome = require("./src/Views/Admin/Home");
var itemlist = require("./src/Views/ItemTTT/List");
var itemadd = require("./src/Views/ItemTTT/Add");
var itemedit = require("./src/Views/ItemTTT/Edit");
var bloglist = require("./src/Views/Blog/List");
var blogedit = require("./src/Views/Blog/Edit");
///////
// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var ttt = {
    common: common,
    admin: {
        home: adminhome,
    },
    itemttt: {
        list: itemlist,
        add: itemadd,
        edit: itemedit,
    },
    blog: {
        list: bloglist,
        edit: blogedit,
    },
};
window.ttt = ttt;

},{"./src/Views/Admin/Home":12,"./src/Views/Blog/Edit":13,"./src/Views/Blog/List":14,"./src/Views/ItemTTT/Add":15,"./src/Views/ItemTTT/Edit":16,"./src/Views/ItemTTT/List":18,"./src/Views/common":19}],3:[function(require,module,exports){
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
    function ItemKO($container, src, fieldNames) {
        var _this = this;
        if (src == null)
            src = { code: null, active: false, features: [], pictures: [] };
        _this = _super.call(this, $container, src, fieldNames) || this;
        var self = _this;
        if (self.price != null)
            common.utils.ensureInteger({ observable: self.price, canBeZero: false, canBeNull: true, mustBePositive: true });
        return _this;
    }
    ItemKO.prototype.toDictObj = function (dict) {
        var rc = _super.prototype.toDictObj.call(this, dict);
        return rc;
    };
    return ItemKO;
}(BaseAutoItem_1.BaseAutoItem));
exports.ItemKO = ItemKO;

},{"../Utils/BaseAutoItem":11,"../Views/common":19}],4:[function(require,module,exports){
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
var TranslationKO = /** @class */ (function () {
    function TranslationKO(src) {
        if (src == null)
            src = { en: '', fr: '', nl: '' };
        this.en = ko.observable(src.en);
        this.fr = ko.observable(src.fr);
        this.nl = ko.observable(src.nl);
    }
    return TranslationKO;
}());
exports.TranslationKO = TranslationKO;
var TranslationItemKO = /** @class */ (function (_super) {
    __extends(TranslationItemKO, _super);
    function TranslationItemKO(src) {
        var _this = this;
        if (src == null)
            src = { en: '', fr: '', nl: '', inOriginal: false, inTranslation: false };
        _this = _super.call(this, src) || this;
        var self = _this;
        _this.inOriginal = ko.observable(src.inOriginal);
        _this.inTranslation = ko.observable(src.inTranslation);
        _this.enOriginal = ko.observable(self.en());
        _this.frOriginal = ko.observable(self.fr());
        _this.nlOriginal = ko.observable(self.nl());
        _this.modified = ko.computed(function () {
            if (self.en() != self.enOriginal())
                return true;
            if (self.fr() != self.frOriginal())
                return true;
            if (self.nl() != self.nlOriginal())
                return true;
            return false;
        });
        return _this;
    }
    TranslationItemKO.prototype.toDTO = function () {
        return {
            en: this.en(),
            fr: this.fr(),
            nl: this.nl(),
        };
    };
    TranslationItemKO.prototype.setAllLanguagesTo = function (str) {
        this.en(str);
        this.fr(str);
        this.nl(str);
    };
    TranslationItemKO.prototype.copyEnToBlanks = function () {
        var self = this;
        var en = self.en();
        if (common.utils.stringIsNullOrWhitespace(self.fr()))
            self.fr(en);
        if (common.utils.stringIsNullOrWhitespace(self.nl()))
            self.nl(en);
    };
    return TranslationItemKO;
}(TranslationKO));
exports.TranslationItemKO = TranslationItemKO;

},{"../Views/common":19}],5:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
var _a;
var common = require("./Views/common");
exports.Languages = (_a = common.utils.strEnum(['en', 'fr', 'nl']), _a.e), exports.allLanguages = _a.a;

},{"./Views/common":19}],6:[function(require,module,exports){
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
function list(p) {
    return __awaiter(this, void 0, void 0, function () {
        var rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, common.url.postRequestForm(common.routes.api.blog.list, p)];
                case 1:
                    rc = _a.sent();
                    rc.posts = rc.result;
                    delete rc.result;
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.list = list;
function uploadPicture(file) {
    return __awaiter(this, void 0, void 0, function () {
        var url, formData, response;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.blog.pictureUpload;
                    formData = new FormData();
                    formData.append('file', file);
                    return [4 /*yield*/, common.url.postRequestFormData(url, formData)];
                case 1:
                    response = _a.sent();
                    response.imageTagContent = response.result;
                    delete response.result;
                    return [2 /*return*/, response];
            }
        });
    });
}
exports.uploadPicture = uploadPicture;

},{"../Views/common":19}],7:[function(require,module,exports){
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
                    url = common.routes.api.items.details.replace(common.routes.itemCodeParameter, code);
                    return [4 /*yield*/, common.url.postRequestJSON(url, {})];
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
                case 0: return [4 /*yield*/, common.url.postRequestJSON(common.routes.api.items.save, p)];
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
function delete_(code) {
    return __awaiter(this, void 0, void 0, function () {
        var url, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.items.delete.replace(common.routes.itemCodeParameter, code);
                    return [4 /*yield*/, common.url.postRequestJSON(url, {})];
                case 1:
                    rc = _a.sent();
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.delete_ = delete_;

},{"../Views/common":19}],8:[function(require,module,exports){
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
function upload(itemCode, file) {
    return __awaiter(this, void 0, void 0, function () {
        var url, formData, response;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.items.pictures.upload.replace(common.routes.itemCodeParameter, itemCode);
                    formData = new FormData();
                    formData.append('file', file);
                    return [4 /*yield*/, common.url.postRequestFormData(url, formData)];
                case 1:
                    response = _a.sent();
                    response.imageNumber = response.result;
                    delete response.result;
                    return [2 /*return*/, response];
            }
        });
    });
}
exports.upload = upload;
function delete_(p) {
    return __awaiter(this, void 0, void 0, function () {
        var url, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.items.pictures.delete.replace(common.routes.itemCodeParameter, p.itemCode);
                    return [4 /*yield*/, common.url.postRequestForm(url, { number: p.number })];
                case 1:
                    rc = _a.sent();
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.delete_ = delete_;
function reorder(p) {
    return __awaiter(this, void 0, void 0, function () {
        var url, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.items.pictures.reorder.replace(common.routes.itemCodeParameter, p.itemCode);
                    return [4 /*yield*/, common.url.postRequestForm(url, { number: p.number, newNumber: p.newNumber })];
                case 1:
                    rc = _a.sent();
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.reorder = reorder;
function setMain(p) {
    return __awaiter(this, void 0, void 0, function () {
        var url, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.items.pictures.setMain.replace(common.routes.itemCodeParameter, p.itemCode);
                    return [4 /*yield*/, common.url.postRequestForm(url, { number: p.number, isMain: p.isMain })];
                case 1:
                    rc = _a.sent();
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.setMain = setMain;

},{"../Views/common":19}],9:[function(require,module,exports){
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
function changePassword(p) {
    return __awaiter(this, void 0, void 0, function () {
        var url, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.changePassword;
                    return [4 /*yield*/, common.url.postRequestForm(url, p)];
                case 1:
                    rc = _a.sent();
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.changePassword = changePassword;

},{"../Views/common":19}],10:[function(require,module,exports){
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
var _a;
var common = require("../Views/common");
exports.TranslationTypes = (_a = common.utils.strEnum(['feature']), _a.e), exports.allTranslationTypes = _a.a;
function list(p) {
    return __awaiter(this, void 0, void 0, function () {
        var rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, common.url.postRequestForm(common.routes.api.translations.list, p)];
                case 1:
                    rc = _a.sent();
                    rc.translations = rc.result;
                    delete rc.result;
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.list = list;
function save(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, common.url.postRequestJSON(common.routes.api.translations.save, p)];
                case 1: return [2 /*return*/, _a.sent()];
            }
        });
    });
}
exports.save = save;
function autoCompleteResolve(p) {
    return __awaiter(this, void 0, void 0, function () {
        var url, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    url = common.routes.api.autoComplete;
                    return [4 /*yield*/, common.url.postRequestForm(url, p)];
                case 1:
                    rc = _a.sent();
                    rc.list = rc.result;
                    delete rc.result;
                    return [2 /*return*/, rc];
            }
        });
    });
}
exports.autoCompleteResolve = autoCompleteResolve;

},{"../Views/common":19}],11:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
var fieldTagAttribute = 'ttt-name';
var BaseAutoItem = /** @class */ (function () {
    function BaseAutoItem($container, src, fieldNames) {
        this.fieldNames = (fieldNames != null) ? fieldNames : [];
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
        // For each tagged elements we find in '$container'
        $container.find("[" + fieldTagAttribute + "]").each(function (i, e) {
            // Extract the field name from that attribute
            var $e = $(e);
            var fieldName = $e.attr(fieldTagAttribute);
            if (self.fieldNames.indexOf(fieldName) >= 0) 
            // That field is already already listed
            { /*NOOP*/ }
            else
                // Add that field to the fields list
                self.fieldNames.push(fieldName);
        });
        // For each listed fields ...
        self.fieldNames.forEach(function (fieldName) {
            // ... create a KO observable on 'this' and set it's value using 'src' ...
            dict[fieldName] = ko.observable(src[fieldName]);
        });
    };
    return BaseAutoItem;
}());
exports.BaseAutoItem = BaseAutoItem;

},{}],12:[function(require,module,exports){
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
var loginCtrl = require("../../Services/LoginController");
var trnCtrl = require("../../Services/TranslationController");
var trnDto = require("../../DTOs/Translation");
var passwordsDontMatchMessage = 'Les 2 mots de passe ne concordent pas';
var passwordChangedMessage = 'Password changed successfully';
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            exports.changePassword = new ChangePassword(p.$passwordBlockingDiv);
            exports.features = new Features(p.$featuresBlockingDiv);
            return [2 /*return*/];
        });
    });
}
exports.init = init;
var ChangePassword = /** @class */ (function () {
    function ChangePassword($blockingDiv) {
        this.$blockingDiv = $blockingDiv;
        this.original = ko.observable('');
        this.newPassword = ko.observable('');
        this.newPasswordAgain = ko.observable('');
    }
    ChangePassword.prototype.change = function () {
        return __awaiter(this, void 0, void 0, function () {
            var self, oldPassword, newPassword, newPasswordAgain, rc;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        self = this;
                        oldPassword = self.original();
                        newPassword = self.newPassword();
                        newPasswordAgain = self.newPasswordAgain();
                        if (common.utils.stringIsNullOrWhitespace(newPassword))
                            return [2 /*return*/];
                        if (common.utils.stringIsNullOrWhitespace(oldPassword))
                            return [2 /*return*/];
                        if (newPassword != newPasswordAgain) {
                            common.html.showMessage(passwordsDontMatchMessage);
                            return [2 /*return*/];
                        }
                        common.html.block(self.$blockingDiv);
                        return [4 /*yield*/, loginCtrl.changePassword({ oldPassword: oldPassword, newPassword: newPassword })];
                    case 1:
                        rc = _a.sent();
                        common.html.unblock(self.$blockingDiv);
                        if (!rc.success) {
                            common.utils.error('change password error', { rc: rc });
                            common.html.showMessage(rc.errorMessage);
                            return [2 /*return*/];
                        }
                        common.html.showMessage(passwordChangedMessage);
                        return [2 /*return*/];
                }
            });
        });
    };
    return ChangePassword;
}());
var TranslationsBase = /** @class */ (function () {
    function TranslationsBase(type, $blockingDiv) {
        var self = this;
        this.$blockingDiv = $blockingDiv;
        this.type = type;
        this.items = ko.observableArray();
        this.hasChanges = ko.computed(function () {
            var items = self.items();
            for (var i = 0; i < items.length; ++i) {
                if (items[i].modified())
                    return true;
            }
            return false;
        });
        this.refresh();
    }
    TranslationsBase.prototype.refresh = function () {
        return __awaiter(this, void 0, void 0, function () {
            var self, rc, items;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        self = this;
                        common.html.block(self.$blockingDiv);
                        return [4 /*yield*/, trnCtrl.list({ type: self.type })];
                    case 1:
                        rc = _a.sent();
                        common.html.unblock(self.$blockingDiv);
                        if (!rc.success) {
                            common.utils.error('refresh translations error', { rc: rc });
                            common.html.showMessage(rc.errorMessage);
                            return [2 /*return*/];
                        }
                        items = rc.translations.map(function (v) { return new TranslationItem(v); });
                        self.items(items);
                        return [2 /*return*/];
                }
            });
        });
    };
    TranslationsBase.prototype.clearLine = function (index) {
        var self = this;
        var item = self.items()[index()];
        item.setAllLanguagesTo('');
    };
    TranslationsBase.prototype.addLine = function () {
        this.items.push(new TranslationItem());
    };
    TranslationsBase.prototype.save = function () {
        return __awaiter(this, void 0, void 0, function () {
            var self, toSave, rc;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        self = this;
                        toSave = self.items()
                            .map(function (item) {
                            if (!item.modified())
                                return null;
                            var dto = item.toDTO();
                            dto.enOriginal = item.enOriginal();
                            return dto;
                        })
                            .filter(function (item) { return (item != null); });
                        if (!(toSave.length == 0)) return [3 /*break*/, 2];
                        // NOOP => Simply refresh
                        return [4 /*yield*/, self.refresh()];
                    case 1:
                        // NOOP => Simply refresh
                        _a.sent();
                        return [2 /*return*/];
                    case 2:
                        common.html.block(self.$blockingDiv);
                        return [4 /*yield*/, trnCtrl.save({ type: self.type, translations: toSave })];
                    case 3:
                        rc = _a.sent();
                        common.html.unblock(self.$blockingDiv);
                        if (!rc.success) {
                            common.utils.error('save translations error', { rc: rc });
                            common.html.showMessage(rc.errorMessage);
                            return [2 /*return*/];
                        }
                        return [4 /*yield*/, self.refresh()];
                    case 4:
                        _a.sent();
                        return [2 /*return*/];
                }
            });
        });
    };
    return TranslationsBase;
}());
var TranslationItem = /** @class */ (function (_super) {
    __extends(TranslationItem, _super);
    function TranslationItem(src) {
        var _this = _super.call(this, src) || this;
        var self = _this;
        _this.showReset = ko.computed(function () { return self.inTranslation(); });
        return _this;
    }
    return TranslationItem;
}(trnDto.TranslationItemKO));
var Features = /** @class */ (function (_super) {
    __extends(Features, _super);
    function Features($blockingDiv) {
        return _super.call(this, trnCtrl.TranslationTypes.feature, $blockingDiv) || this;
    }
    return Features;
}(TranslationsBase));

},{"../../DTOs/Translation":4,"../../Services/LoginController":9,"../../Services/TranslationController":10,"../common":19}],13:[function(require,module,exports){
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
var ctrl = require("../../Services/BlogController");
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            common.utils.log('edit.init() START', { p: p });
            common.utils.log('edit.init(): Init picture upload');
            p.$picUploadControl.on('change', function () {
                var files = p.$picUploadControl[0].files;
                uploadPicture(files);
            });
            common.utils.log('edit.init() END');
            return [2 /*return*/];
        });
    });
}
exports.init = init;
function uploadPicture(files) {
    return __awaiter(this, void 0, void 0, function () {
        var rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.uploadPicture(): START', { files: files });
                    if (files.length == 0)
                        return [2 /*return*/];
                    if (window.FormData === undefined) {
                        common.utils.error("This browser doesn't support HTML5 file uploads!");
                        return [2 /*return*/];
                    }
                    return [4 /*yield*/, ctrl.uploadPicture(files[0])];
                case 1:
                    rc = _a.sent();
                    if (!rc.success) {
                        common.utils.error('edit.uploadPicture(): Error', { rc: rc });
                        common.html.showMessage(rc.errorMessage);
                        return [2 /*return*/];
                    }
                    $('#foobar').html(rc.imageTagContent);
                    common.utils.log('edit.uploadPicture(): END');
                    return [2 /*return*/];
            }
        });
    });
}

},{"../../Services/BlogController":6,"../common":19}],14:[function(require,module,exports){
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
var ctrl = require("../../Services/BlogController");
var blogIdAttribute = 'ttt-blog-id';
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('list.init() START', { p: p });
                    return [4 /*yield*/, loadImages()];
                case 1:
                    _a.sent();
                    common.utils.log('list.init() END');
                    return [2 /*return*/];
            }
        });
    });
}
exports.init = init;
function loadImages() {
    return __awaiter(this, void 0, void 0, function () {
        var posts, tasks;
        var _this = this;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('loadImages(): START');
                    posts = [];
                    $("[" + blogIdAttribute + "]").each(function (i, e) {
                        var $container = $(e);
                        var id = parseInt($container.attr(blogIdAttribute));
                        $container.removeAttr(blogIdAttribute); // Remove the "tag attribute" from the DOM so that next call to this method does not re-find this item
                        posts.push({ id: id, $container: $container, model: null });
                    });
                    common.utils.log('loadImages()', { posts: posts });
                    tasks = posts.map(function (item) { return __awaiter(_this, void 0, void 0, function () {
                        var rc;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    common.html.block(item.$container);
                                    return [4 /*yield*/, ctrl.list({ id: item.id })];
                                case 1:
                                    rc = _a.sent();
                                    common.html.unblock(item.$container);
                                    if (!rc.success) {
                                        common.utils.error('retreive post error', { rc: rc });
                                        common.html.showMessage(rc.errorMessage);
                                        return [2 /*return*/];
                                    }
                                    item.model = rc.posts[0];
                                    ko.applyBindings(item.model, item.$container[0]);
                                    return [2 /*return*/];
                            }
                        });
                    }); });
                    common.utils.log('loadImages(): Wait for all tasks to terminate');
                    return [4 /*yield*/, Promise.all(tasks)];
                case 1:
                    _a.sent();
                    common.utils.log('loadImages(): END');
                    return [2 /*return*/];
            }
        });
    });
}

},{"../../Services/BlogController":6,"../common":19}],15:[function(require,module,exports){
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
var $blockingDiv;
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            common.utils.log('add.init() START', { p: p });
            $blockingDiv = p.$blockingDiv;
            common.utils.log('add.init(): Create KO item');
            exports.item = new dto.ItemKO(p.$fieldsContainer);
            common.utils.log('add.init(): Apply KO item');
            ko.applyBindings(exports.item, p.$fieldsContainer[0]);
            common.utils.log('add.init(): Bind JQuery events');
            p.$btnSave.click(save);
            common.utils.log('add.init() END');
            return [2 /*return*/];
        });
    });
}
exports.init = init;
function save() {
    return __awaiter(this, void 0, void 0, function () {
        var obj, rc, url;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('add.save(): START');
                    obj = exports.item.toDictObj();
                    common.utils.log('add.save(): Launch save');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, ctrl.save({ item: obj })];
                case 1:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    if (!rc.success) {
                        common.utils.error('add.save(): Error', { rc: rc });
                        common.html.showMessage(rc.errorMessage);
                        return [2 /*return*/];
                    }
                    common.utils.log('add.save(): Redirect', { rc: rc });
                    url = common.routes.itemTTT.edit.replace(common.routes.itemCodeParameter, rc.newCode);
                    common.utils.log("add.save(): url=\"" + url + "\"");
                    common.url.redirect(url);
                    common.utils.log('add.save(): END');
                    return [2 /*return*/];
            }
        });
    });
}

},{"../../DTOs/Item":3,"../../Services/ItemController":7,"../common":19}],16:[function(require,module,exports){
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
var picCtrl = require("../../Services/ItemPictureController");
var trnCtrl = require("../../Services/TranslationController");
var Translation_1 = require("../../DTOs/Translation");
var message_confirmDelete = 'Are you sure you want to delete that item?';
var message_saveSuccess = 'Item saved successfully';
var message_refreshFailed = 'An error occured while refresing the data: ';
var message_confirmDeletePicture = 'Are you sure you want to delete that image?';
var $blockingDiv;
var originalCode;
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            common.utils.log('edit.init() START', { p: p });
            $blockingDiv = p.$blockingDiv;
            common.utils.log('edit.init(): Create KO item');
            exports.item = new ItemKO(p.$fieldsContainer, p.model);
            originalCode = exports.item.code();
            common.utils.log('edit.init(): Bind JQuery events');
            p.$btnSave.click(save);
            p.$btnDelete.click(function () { delete_(); });
            common.utils.log('edit.init(): Init picture upload');
            initPictureUpload(p.$picUploadDropZone, p.$picUploadControl);
            common.utils.log('edit.init() END');
            return [2 /*return*/];
        });
    });
}
exports.init = init;
function initPictureUpload($dropZone, $upload) {
    common.utils.log('edit.initDropZone(): Create drop zone object');
    exports.picDropZone = {
        hover: ko.observable(false),
    };
    common.utils.log('edit.initDropZone(): Watch for drag-drops on "upload image drop zone"');
    $dropZone.on('dragenter', function (e) {
        exports.picDropZone.hover(true);
        e.stopPropagation();
        e.preventDefault();
    });
    $dropZone.on('dragover', function (e) {
        exports.picDropZone.hover(true);
        e.stopPropagation();
        e.preventDefault();
    });
    $dropZone.on('dragleave', function (e) {
        exports.picDropZone.hover(false);
    });
    $dropZone.on('drop', function (e) {
        exports.picDropZone.hover(false);
        e.preventDefault();
        var files = e.originalEvent.dataTransfer.files;
        uploadPicture(files);
    });
    $upload.on('change', function () {
        var files = $upload[0].files;
        uploadPicture(files);
    });
}
function autoCompleteFeature(searchTerm) {
    return __awaiter(this, void 0, void 0, function () {
        var rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, trnCtrl.autoCompleteResolve({ type: trnCtrl.TranslationTypes.feature, searchString: searchTerm, includeTranslations: true })];
                case 1:
                    rc = _a.sent();
                    if (!rc.success) {
                        common.utils.error('autocomplete error', { rc: rc });
                        return [2 /*return*/, []];
                    }
                    return [2 /*return*/, rc.list];
            }
        });
    });
}
exports.autoCompleteFeature = autoCompleteFeature;
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
                    if (!rc.success) {
                        common.utils.error('edit.save(): Error', { rc: rc });
                        common.html.showMessage(rc.errorMessage);
                        return [2 /*return*/];
                    }
                    common.utils.log('edit.save(): Refresh values', { rc: rc });
                    return [4 /*yield*/, refresh(rc.newCode)];
                case 2:
                    rcr = _a.sent();
                    if (rcr)
                        common.html.showMessage(message_saveSuccess);
                    common.utils.log('edit.save(): END');
                    return [2 /*return*/];
            }
        });
    });
}
function delete_(confirmed) {
    return __awaiter(this, void 0, void 0, function () {
        var rc_1, rc, url;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.delete(): START');
                    if (!(confirmed != true)) return [3 /*break*/, 2];
                    common.utils.log('edit.delete(): Ask confirmation');
                    return [4 /*yield*/, common.html.confirm(message_confirmDelete)];
                case 1:
                    rc_1 = _a.sent();
                    if (!rc_1) {
                        common.utils.log('edit.delete(): NOT CONFIRMED');
                        return [2 /*return*/];
                    }
                    _a.label = 2;
                case 2:
                    common.utils.log('edit.delete(): Launch delete');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, ctrl.delete_(originalCode)];
                case 3:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    if (!rc.success) {
                        common.utils.error('edit.delete(): Error', { rc: rc });
                        common.html.showMessage(rc.errorMessage);
                        return [2 /*return*/];
                    }
                    common.utils.log('edit.delete(): Redirect');
                    url = common.routes.itemTTT.list.replace(common.routes.languageParameter, common.pageParameters.currentLanguage);
                    common.utils.log('edit.delete()', { url: url });
                    common.url.redirect(url);
                    common.utils.log('edit.delete(): END');
                    return [2 /*return*/];
            }
        });
    });
}
function uploadPicture(files) {
    return __awaiter(this, void 0, void 0, function () {
        var tasks, rcs;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.uploadPicture(): START', { files: files });
                    if (files.length == 0)
                        return [2 /*return*/];
                    if (window.FormData === undefined) {
                        common.utils.error("This browser doesn't support HTML5 file uploads!");
                        return [2 /*return*/];
                    }
                    common.utils.log("edit.uploadPicture(): Create '" + files.length + "' upload tasks");
                    common.html.block($blockingDiv);
                    tasks = $.map(files, function (file) {
                        return picCtrl.upload(originalCode, file);
                    });
                    common.utils.log('edit.uploadPicture(): Wait for tasks to terminate');
                    return [4 /*yield*/, Promise.all(tasks)];
                case 1:
                    rcs = _a.sent();
                    common.html.unblock($blockingDiv);
                    common.utils.log('edit.uploadPicture(): All tasks terminated');
                    $.each(rcs, function (i, rc) {
                        if (!rc) {
                            common.utils.error('edit.delete(): Upload error', { i: i, rc: rc });
                            common.html.showMessage(rc.errorMessage);
                        }
                    });
                    return [4 /*yield*/, refresh()];
                case 2:
                    _a.sent();
                    common.utils.log('edit.uploadPicture(): END');
                    return [2 /*return*/];
            }
        });
    });
}
function deletePicture(picture, confirmed) {
    return __awaiter(this, void 0, void 0, function () {
        var rc_2, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.deletePicture(): START');
                    if (!(confirmed != true)) return [3 /*break*/, 2];
                    common.utils.log('edit.deletePicture(): Ask confirmation');
                    return [4 /*yield*/, common.html.confirm(message_confirmDeletePicture)];
                case 1:
                    rc_2 = _a.sent();
                    if (!rc_2) {
                        common.utils.log('edit.deletePicture(): NOT CONFIRMED');
                        return [2 /*return*/];
                    }
                    _a.label = 2;
                case 2:
                    common.utils.log('edit.deletePicture(): Launch delete request');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, picCtrl.delete_({ itemCode: picture.itemCode, number: picture.number })];
                case 3:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    if (!rc.success) {
                        common.utils.error('edit.deletePicture(): Error', { rc: rc });
                        common.html.showMessage(rc.errorMessage);
                    }
                    common.utils.log('edit.deletePicture(): Launch refresh');
                    return [4 /*yield*/, refresh()];
                case 4:
                    _a.sent();
                    common.utils.log('edit.deletePicture(): END');
                    return [2 /*return*/];
            }
        });
    });
}
exports.deletePicture = deletePicture;
function toggleMainPicture(picture) {
    return __awaiter(this, void 0, void 0, function () {
        var rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.toggleMainPicture(): START');
                    common.utils.log('edit.toggleMainPicture(): Launch toggle request');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, picCtrl.setMain({ itemCode: picture.itemCode, number: picture.number, isMain: (!picture.isMainImage) })];
                case 1:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    if (!rc.success) {
                        common.utils.error('edit.toggleMainPicture(): Error', { rc: rc });
                        common.html.showMessage(rc.errorMessage);
                    }
                    common.utils.log('edit.toggleMainPicture(): Launch refresh');
                    return [4 /*yield*/, refresh()];
                case 2:
                    _a.sent();
                    common.utils.log('edit.toggleMainPicture(): END');
                    return [2 /*return*/];
            }
        });
    });
}
exports.toggleMainPicture = toggleMainPicture;
function reorderPicture(picture, offset) {
    return __awaiter(this, void 0, void 0, function () {
        var newNumber, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('edit.movePicture(): START');
                    newNumber = picture.number + offset;
                    if (newNumber <= 0)
                        newNumber = 1;
                    if (picture.number == newNumber)
                        // NOOP
                        return [2 /*return*/];
                    common.utils.log('edit.movePicture(): Launch move request');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, picCtrl.reorder({ itemCode: picture.itemCode, number: picture.number, newNumber: newNumber })];
                case 1:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    if (!rc.success) {
                        common.utils.error('edit.movePicture(): Error', { rc: rc });
                        common.html.showMessage(rc.errorMessage);
                    }
                    common.utils.log('edit.movePicture(): Launch refresh');
                    return [4 /*yield*/, refresh()];
                case 2:
                    _a.sent();
                    common.utils.log('edit.movePicture(): END');
                    return [2 /*return*/];
            }
        });
    });
}
exports.reorderPicture = reorderPicture;
function refresh(code) {
    return __awaiter(this, void 0, void 0, function () {
        var newUrl, rc;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    // TODO: Allow refresh only images to avoid overwriting any modifications to input fields ...
                    common.utils.log('edit.refresh(): START');
                    newUrl = null;
                    if (code == null) {
                        code = originalCode;
                    }
                    else {
                        if (originalCode != code)
                            // URL must change!
                            newUrl = common.routes.itemTTT.edit.replace(common.routes.itemCodeParameter, code);
                        originalCode = code;
                    }
                    common.utils.log('edit.refresh(): Launch request');
                    common.html.block($blockingDiv);
                    return [4 /*yield*/, ctrl.details(code)];
                case 1:
                    rc = _a.sent();
                    common.html.unblock($blockingDiv);
                    if (!rc.success) {
                        common.utils.error('edit.refresh(): Error', { rc: rc });
                        common.html.showMessage(message_refreshFailed + rc.errorMessage);
                        return [2 /*return*/, false];
                    }
                    common.utils.log('edit.refresh(): Load from new item', { rc: rc });
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
/** Used to add a changing parameter to the URL to bypass browser's cache */
function scrambleUrl(url) {
    if (url.indexOf('?') >= 0)
        url = url + '&';
    else
        url = url + '?';
    url = url + 'p=' + common.utils.newGuid();
    return url;
}
exports.scrambleUrl = scrambleUrl;
var ItemKO = /** @class */ (function (_super) {
    __extends(ItemKO, _super);
    function ItemKO($container, src) {
        var _this = _super.call(this, $container, src) || this;
        _this.features = ko.observableArray(src.features.map(function (v) { return new Translation_1.TranslationKO(v); }));
        _this.pictures = ko.observableArray(src.pictures);
        return _this;
    }
    ItemKO.prototype.addNewFeature = function () {
        var self = this;
        self.features.push(new Translation_1.TranslationKO());
    };
    ItemKO.prototype.toDictObj = function (dict) {
        var self = this;
        var item = _super.prototype.toDictObj.call(this, dict);
        item.features = self.features().map(function (v) { return { en: v.en(), fr: '', nl: '' }; }); // nb: only EN is used server-side
        return item;
    };
    ItemKO.prototype.fromDictObj = function (item) {
        var self = this;
        _super.prototype.fromDictObj.call(this, item);
        self.features(item.features.map(function (v) { return new Translation_1.TranslationKO(v); }));
        self.pictures(item.pictures);
    };
    return ItemKO;
}(dto.ItemKO));

},{"../../DTOs/Item":3,"../../DTOs/Translation":4,"../../Services/ItemController":7,"../../Services/ItemPictureController":8,"../../Services/TranslationController":10,"../common":19}],17:[function(require,module,exports){
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
                        url = common.routes.itemTTT.list.replace(common.routes.languageParameter, common.pageParameters.currentLanguage);
                        return [4 /*yield*/, common.url.getRequest(url, p)];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    }
    list.getListContent = getListContent;
    ;
})(list = exports.list || (exports.list = {}));

},{"../common":19}],18:[function(require,module,exports){
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
var itemCtrl = require("../../Services/ItemController");
var ItemTTTController_1 = require("./ItemTTTController");
var dto = require("../../DTOs/Item");
var Language_1 = require("../../Language");
var modelSearchString = '[type=ttt-model]';
var currentSortingField;
var $txtSearch;
var $cbShowInactive;
var $divCarsList;
var $divBlocking;
function init(p) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            common.utils.log('list.init(): START');
            currentSortingField = p.sortingField;
            $txtSearch = p.$txtSearch;
            $cbShowInactive = p.$cbShowInactive.prop('checked', true);
            $divCarsList = p.$divCarsList;
            $divBlocking = $divCarsList;
            p.$btnViewGrid.click(function () { refreshList({ viewMode: ItemTTTController_1.list.ViewModes.grid }); });
            p.$btnViewList.click(function () { refreshList({ viewMode: ItemTTTController_1.list.ViewModes.list }); });
            p.$btnSortName.click(function () { refreshList({ sortingField: ItemTTTController_1.list.SortingFields.name }); });
            p.$btnSortPrice.click(function () { refreshList({ sortingField: ItemTTTController_1.list.SortingFields.price }); });
            reconstructItemsList();
            common.utils.log('list.init(): Bind JQuery events');
            $txtSearch.keyup(function () { filterItemsList(); });
            $cbShowInactive.change(function () { filterItemsList(); });
            common.utils.log('list.init(): END');
            return [2 /*return*/];
        });
    });
}
exports.init = init;
function reconstructItemsList() {
    common.utils.log('list.reconstructItemsList(): START');
    var $models = $divCarsList.find(modelSearchString);
    common.utils.log("list.reconstructItemsList(): Reconstructing " + $models.length + " models");
    var items = [];
    $models.each(function (i, e) {
        // Find the JSON model's tag & set its parent element as the item's container
        var $modelElement = $(e);
        var $container = $modelElement.parent();
        var model = JSON.parse($modelElement.text());
        // Remove the model tag (i.e. free memory ?)
        $modelElement.remove();
        // Create the ItemKO and add it to the list
        var item = new ItemKO($container, model);
        items.push(item);
    });
    common.utils.log('list.reconstructItemsList(): Replacing itemsList');
    exports.itemsList = items;
    common.utils.log('list.reconstructItemsList(): END');
}
function filterItemsList() {
    exports.itemsList.forEach(function (item) {
        item.visible(item.getIsVisible());
    });
}
exports.filterItemsList = filterItemsList;
function refreshList(p) {
    return __awaiter(this, void 0, void 0, function () {
        var html;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    common.utils.log('list.refreshList(): START', { p: p, currentSortingField: currentSortingField });
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
                    common.utils.log('list.refreshList(): Request html & replace list DOM');
                    common.html.block($divBlocking);
                    return [4 /*yield*/, ItemTTTController_1.list.getListContent(p)];
                case 1:
                    html = _a.sent();
                    common.html.unblock($divBlocking);
                    $divCarsList.html(html);
                    reconstructItemsList();
                    common.utils.log('list.refreshList(): END', { currentSortingField: currentSortingField });
                    return [2 /*return*/];
            }
        });
    });
}
var ItemKO = /** @class */ (function (_super) {
    __extends(ItemKO, _super);
    function ItemKO($container, model) {
        var _this = this;
        var required = [common.utils.nameof('code'),
            common.utils.nameof('name'),
            common.utils.nameof('descriptionEN'),
            common.utils.nameof('descriptionFR'),
            common.utils.nameof('descriptionNL'),
            common.utils.nameof('active')];
        _this = _super.call(this, $container, model, required) || this;
        var self = _this;
        _this.$container = $container;
        _this.visible = ko.observable(self.getIsVisible());
        // Bind this object to its container
        ko.applyBindings(self, $container[0]);
        return _this;
    }
    ItemKO.prototype.toggleActive = function () {
        return __awaiter(this, void 0, void 0, function () {
            var self, rc, model, rc;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        common.utils.log('list.ItemKO.toggleActive(): START');
                        self = this;
                        common.utils.log('list.ItemKO.toggleActive(): Retreive original model');
                        common.html.block($divBlocking);
                        return [4 /*yield*/, itemCtrl.details(self.code())];
                    case 1:
                        rc = _a.sent();
                        if (!rc.success) {
                            common.html.unblock($divBlocking);
                            common.utils.error('Item retreive error', { rc: rc });
                            return [2 /*return*/];
                        }
                        model = rc.item;
                        common.utils.log('list.ItemKO.toggleActive(): Toggle active flag & save');
                        model.active = (!model.active);
                        return [4 /*yield*/, itemCtrl.save({ originalCode: model.code, item: model })];
                    case 2:
                        rc = _a.sent();
                        if (!rc.success)
                            common.utils.error('Item save error', { rc: rc });
                        common.utils.log('list.ItemKO.toggleActive(): Refresh');
                        return [4 /*yield*/, refreshList({})];
                    case 3:
                        _a.sent();
                        common.html.unblock($divBlocking);
                        common.utils.log('list.ItemKO.toggleActive(): END');
                        return [2 /*return*/];
                }
            });
        });
    };
    ItemKO.prototype.getIsVisible = function () {
        var self = this;
        // Check "show active" checkbox
        var showInactive = $cbShowInactive.is(':checked');
        if (!self.active())
            if (!showInactive)
                return false;
        // Check "search" textbox
        var searchString = $txtSearch.val();
        searchString = (searchString == null ? '' : searchString).toLowerCase().trim();
        if (searchString == '')
            return true;
        var searchWords = [];
        $.each(searchString.split(' '), function (i, word) {
            if (common.utils.stringIsNullOrWhitespace(word))
                return;
            searchWords.push(word.trim());
        });
        var searchFields = [];
        searchFields.push(self.name());
        switch (common.pageParameters.currentLanguage) {
            case Language_1.Languages.en:
                searchFields.push(self.descriptionEN());
                break;
            case Language_1.Languages.fr:
                searchFields.push(self.descriptionFR());
                break;
            case Language_1.Languages.nl:
                searchFields.push(self.descriptionNL());
                break;
        }
        var matchCount = 0;
        searchWords.forEach(function (word, i) {
            for (var i_1 = 0; i_1 < searchFields.length; ++i_1) {
                var field = searchFields[i_1].toLowerCase();
                if (field.indexOf(word) >= 0) {
                    ++matchCount;
                    return;
                }
            }
        });
        if (matchCount != searchWords.length) // All words must match
            return false;
        return true;
    };
    return ItemKO;
}(dto.ItemKO));

},{"../../DTOs/Item":3,"../../Language":5,"../../Services/ItemController":7,"../common":19,"./ItemTTTController":17}],19:[function(require,module,exports){
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
function __export(m) {
    for (var p in m) if (!exports.hasOwnProperty(p)) exports[p] = m[p];
}
Object.defineProperty(exports, "__esModule", { value: true });
var common = require("./common");
exports.debugMessages = false; // NB: 'export' so that it can be easily changed from the browser's console
function init(p) {
    var _this = this;
    exports.pageParameters = p.pageParameters;
    exports.routes = exports.pageParameters.routes;
    utils.log('common.init()');
    utils.log('common.init(): Add custom KnockoutHandler');
    ko.bindingHandlers.ttt_autocomplete =
        {
            init: function (element, valueAccessor) {
                var searchFunction = valueAccessor();
                $(element).autocomplete({ minLength: 0,
                    source: function (request, resolve) { return __awaiter(_this, void 0, void 0, function () {
                        var list;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, searchFunction(request.term)];
                                case 1:
                                    list = _a.sent();
                                    resolve(list);
                                    return [2 /*return*/];
                            }
                        });
                    }); },
                });
            },
        };
    // cf. https://knockoutjs.com/examples/animatedTransitions.html
    ko.bindingHandlers.ttt_slideUpDownVisible =
        {
            init: function (element, valueAccessor) {
                var value = ko.unwrap(valueAccessor());
                if (!value)
                    $(element).hide();
            },
            update: function (element, valueAccessor) {
                var value = ko.unwrap(valueAccessor());
                if (value)
                    $(element).slideDown();
                else
                    $(element).slideUp();
            }
        };
    ko.bindingHandlers.ttt_slideLeftRightVisible =
        {
            init: function (element, valueAccessor) {
                var value = ko.unwrap(valueAccessor());
                if (!value)
                    $(element).hide();
            },
            update: function (element, valueAccessor) {
                var value = ko.unwrap(valueAccessor());
                if (value)
                    $(element).show('blind', { direction: 'left' });
                else
                    $(element).hide('blind', { direction: 'left' });
            }
        };
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
    /** cf. https://schneidenbach.gitbooks.io/typescript-cookbook/nameof-operator.html */
    utils.nameof = function (name) { return name; };
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
        if (p.canBeNull == null)
            p.canBeNull = false;
        if (p.fallbackValue == null) {
            if (p.canBeZero)
                p.fallbackValue = 0;
            else
                p.fallbackValue = 1;
        }
        var canBeNull = p.canBeNull;
        p.observable.subscribe(function (value) {
            if (canBeNull) {
                if ((value == '') || (value == null)) {
                    p.observable(null);
                    return;
                }
            }
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
(function (html_1) {
    function showMessage(msg) {
        // Bootstrap style:
        var html = "<div class=\"modal\" tabindex=\"-1\" role=\"dialog\">\n\t\t\t\t\t\t<div class=\"modal-dialog\" role=\"document\">\n\t\t\t\t\t\t\t<div class=\"modal-content\">\n\t\t\t\t\t\t\t\t<div class=\"modal-body\">\n\t\t\t\t\t\t\t\t\t<!-- Text content here -->\n\t\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t\t\t<div class=\"modal-footer\">\n\t\t\t\t\t\t\t\t\t<button type=\"button\" class=\"btn btn-secondary\" data-dismiss=\"modal\">Close</button>\n\t\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t</div>\n\t\t\t\t\t</div>";
        var $div = $(html);
        $div.find('.modal-body').text(msg);
        $div.modal();
        // JQuery style:
        // var $div = $('<div/>').text('Hello world');
        // $('body').append( $div );
        // $div.dialog()
    }
    html_1.showMessage = showMessage;
    function confirm(msg) {
        // Bootstrap style:
        var html = "<div class=\"modal\" tabindex=\"-1\" role=\"dialog\">\n\t\t\t\t\t\t<div class=\"modal-dialog\" role=\"document\">\n\t\t\t\t\t\t\t<div class=\"modal-content\">\n\t\t\t\t\t\t\t\t<div class=\"modal-body\">\n\t\t\t\t\t\t\t\t\t<!-- Text content here -->\n\t\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t\t\t<div class=\"modal-footer\">\n\t\t\t\t\t\t\t\t\t<button type=\"button\" class=\"btn btn-primary\">Save changes</button>\n\t\t\t\t\t\t\t\t\t<button type=\"button\" class=\"btn btn-secondary\" data-dismiss=\"modal\">Cancel</button>\n\t\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t</div>\n\t\t\t\t\t</div>";
        var $div = $(html);
        $div.find('.modal-body').text(msg);
        return new Promise(function (resolve) {
            var confirmed = false;
            $div.find('.btn-primary').click(function () {
                confirmed = true;
                $div.modal('hide');
                resolve(true);
            });
            $div.on('hidden.bs.modal', function (e) {
                if (confirmed)
                    // Clicked on 'Save' => NOOP
                    // nb: should not happen, but in case ...
                    return;
                // Manually closed => Cancel
                resolve(false);
            });
            $div.modal();
        });
        // JQuery style:
        // TODO ...
    }
    html_1.confirm = confirm;
    /** Invoke jQuery.blockUI's '.block()' on the specified element but supports multiple invokation on the same element */
    function block($e) {
        // Insert/increment a block counter as jQuery 'data()'
        var blockCounter = ($e.data('ttt_blockCounter') | 0) + 1;
        $e.data('ttt_blockCounter', blockCounter);
        if (blockCounter == 1)
            // This element is not blocked yet
            $e.block(); // TODO: ACA: jQuery.blockUI typings ...
        return $e;
    }
    html_1.block = block;
    /** Invoke jQuery.blockUI's '.unblock()' on the specified element except if it has been block()ed more than once */
    function unblock($e) {
        // Decrement the block counter in the jQuery 'data()'
        var blockCounter = ($e.data('ttt_blockCounter') | 0) - 1;
        $e.data('ttt_blockCounter', blockCounter);
        if (blockCounter < 0) {
            // There is a logic error somewhere...
            common.utils.error('INTERNAL ERROR: Unblock count > block count:', blockCounter);
            // Reset counter
            blockCounter = 0;
            $e.data('ttt_blockCounter', 0);
        }
        if (blockCounter == 0)
            // This element is no more blocked by anything else
            $e.unblock(); // TODO: ACA: jQuery.blockUI typings ...
        return $e;
    }
    html_1.unblock = unblock;
    function ensureVisible($e) {
        // Scroll
        var offset = $e.offset().top - (20 + $e.height());
        $('html, body').animate({ scrollTop: offset }); // cf.: https://stackoverflow.com/questions/4884839/how-do-i-get-an-element-to-scroll-into-view-using-jquery?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        // Blink
        $e.effect("highlight", {}, 2000); // cf.: https://stackoverflow.com/questions/5205445/jquery-blinking-highlight-effect-on-div?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
    }
    html_1.ensureVisible = ensureVisible;
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
    function postRequestForm(url, request) {
        utils.log('postRequestForm', { url: url, request: request });
        return new Promise(function (resolve, reject) {
            $.ajax({ type: 'POST',
                url: url,
                contentType: 'application/x-www-form-urlencoded',
                data: request,
                dataType: 'json',
                success: function (data, textStatus, jqXHR) {
                    utils.log('postRequestForm', { response: data });
                    resolve(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    utils.error('postRequestForm rejected', { jqXHR: jqXHR, textStatus: textStatus, errorThrown: errorThrown });
                    reject(textStatus);
                }
            });
        });
    }
    url_1.postRequestForm = postRequestForm;
    function postRequestFormData(url, formData) {
        utils.log('postRequestFormData', { url: url, formData: formData });
        return new Promise(function (resolve, reject) {
            $.ajax({ type: "POST",
                url: url,
                contentType: false,
                processData: false,
                data: formData,
                dataType: 'json',
                success: function (data, textStatus, jqXHR) {
                    utils.log('postRequestFormData', { response: data });
                    resolve(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    utils.error('postRequestFormData rejected', { jqXHR: jqXHR, textStatus: textStatus, errorThrown: errorThrown });
                    reject(textStatus);
                },
            });
        });
    }
    url_1.postRequestFormData = postRequestFormData;
    function postRequestJSON(url, request) {
        utils.log('postRequestJSON', { url: url, request: request });
        var requestStr = JSON.stringify(request);
        return new Promise(function (resolve, reject) {
            $.ajax({ type: 'POST',
                url: url,
                contentType: 'application/json',
                data: requestStr,
                dataType: 'json',
                success: function (data, textStatus, jqXHR) {
                    utils.log('postRequestJSON', { response: data });
                    resolve(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    utils.error('postRequestJSON rejected', { jqXHR: jqXHR, textStatus: textStatus, errorThrown: errorThrown });
                    reject(textStatus);
                }
            });
        });
    }
    url_1.postRequestJSON = postRequestJSON;
    function getRequest(url, request) {
        if (request != null) {
            var parms = stringifyParameters(request);
            url = url + "?" + parms;
        }
        utils.log('getRequest', { url: url, request: request });
        return new Promise(function (resolve, reject) {
            $.ajax({ type: 'GET',
                url: url,
                contentType: 'text/html',
                success: function (data, textStatus, jqXHR) {
                    utils.log('getRequest', { response: data });
                    resolve(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    utils.error('getRequest rejected', { jqXHR: jqXHR, textStatus: textStatus, errorThrown: errorThrown });
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

},{"../Language":5,"./common":19}]},{},[1,2])

//# sourceMappingURL=site.js.map
