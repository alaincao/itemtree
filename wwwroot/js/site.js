(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
const common = require("./common");
exports.debugMessages = false; // NB: 'export' so that it can be easily changed from the browser's console
function init(p) {
    common.utils.log('INIT', { pageParameters: p.pageParameters });
}
exports.init = init;
var utils;
(function (utils) {
    function log(...optionalParams) {
        console.log.apply(console, arguments);
    }
    utils.log = log;
    function error(...optionalParams) {
        console.log.apply(console, arguments);
    }
    utils.error = error;
    /** Function to simulate string-valued enums
     * Based on: https://basarat.gitbooks.io/typescript/docs/types/literal-types.html
     * Returns: { e:the enum , a:the array specified as parameter } */
    function strEnum(a) {
        const e = a.reduce((res, key) => {
            res[key] = key;
            return res;
        }, Object.create(null));
        return { e, a };
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
            return $.map(txt, v => $(document.createElement('span')).text(v).html()).join('<br/>');
        else
            return $(document.createElement('span')).text(txt).html();
    }
    utils.htmlEncode = htmlEncode;
    function arrayContains(a, item) {
        return (a.indexOf(item) >= 0);
    }
    utils.arrayContains = arrayContains;
    function arrayUnique(a) {
        //return a.filter( (v,i)=>( a.indexOf(v) === i ) );
        return [...new Set(a)];
    }
    utils.arrayUnique = arrayUnique;
    function arrayRemoveItem(a, item) {
        let i = a.indexOf(item);
        if (i < 0)
            return false;
        a.splice(i, 1);
        return true;
    }
    utils.arrayRemoveItem = arrayRemoveItem;
    function arraySum(a, f) {
        let rc = 0;
        a.forEach(function (e) {
            rc += f(e);
        });
        return rc;
    }
    utils.arraySum = arraySum;
    function arrayMoveItem(p) {
        // NB: Does not yet work when n>1 ...
        let n = 1;
        if (p.direction != null)
            n = (p.direction == 'up') ? -n : n;
        const i = p.list.indexOf(p.item); // Start position
        if (i < 0)
            throw { error: 'Could not find item in list', item: p.item };
        let j = i + n; // End position
        if (j < 0)
            j = 0;
        else if (j >= p.list.length)
            j = p.list.length - 1;
        if (i == j)
            // NOOP
            return p.list;
        // Swap items here
        const tmp = p.list[i];
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
})(utils = exports.utils || (exports.utils = {})); // namespace utils
var html;
(function (html) {
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
        const offset = $e.offset().top - (20 + $e.height());
        $('html, body').animate({ scrollTop: offset }); // cf.: https://stackoverflow.com/questions/4884839/how-do-i-get-an-element-to-scroll-into-view-using-jquery?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        // Blink
        $e.effect("highlight", {}, 2000); // cf.: https://stackoverflow.com/questions/5205445/jquery-blinking-highlight-effect-on-div?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
    }
    html.ensureVisible = ensureVisible;
})(html = exports.html || (exports.html = {})); // namespace html
var url;
(function (url_1) {
    const onChangedEvent = 'onUrlChanged';
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
            const queryString = stringifyParameters(p.parameters);
            if (queryString.length > 0)
                newPath = newPath + '?' + queryString;
        }
        return newPath;
    }
    url_1.createUrl = createUrl;
    /** Change the browser's current URL without triggering an HTTP request (NB: Will trigger any registered 'onChanged()' callbacks) */
    function pushHistory(p) {
        const newPath = createUrl(p);
        window.history.pushState({}, p.newTitle, newPath);
        // Invoke any registered callbacks
        onChangedCallbacks.trigger(onChangedEvent);
    }
    url_1.pushHistory = pushHistory;
    function postRequest(url, request) {
        if (exports.debugMessages)
            request.debug = true;
        let requestStr = JSON.stringify(request);
        return new Promise((resolve, reject) => {
            $.ajax({ type: 'POST',
                url: url,
                contentType: 'application/json',
                data: requestStr,
                dataType: 'json',
                success: (data, textStatus, jqXHR) => resolve(data),
                error: (jqXHR, textStatus, errorThrown) => {
                    reject(textStatus);
                }
            });
        });
    }
    url_1.postRequest = postRequest;
    function downloadFile(url) {
        $(document.body).append($("<iframe/>").attr({ src: url,
            style: 'visibility:hidden;display:none' }));
    }
    url_1.downloadFile = downloadFile;
})(url = exports.url || (exports.url = {})); // namespace url

},{"./common":1}],2:[function(require,module,exports){

},{}],3:[function(require,module,exports){
Object.defineProperty(exports, "__esModule", { value: true });
const common = require("./Views/common");
///////
// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var itemttt = {
    common: common,
    api: {},
};
window.itemttt = itemttt;

},{"./Views/common":1}]},{},[2,3])

//# sourceMappingURL=site.js.map