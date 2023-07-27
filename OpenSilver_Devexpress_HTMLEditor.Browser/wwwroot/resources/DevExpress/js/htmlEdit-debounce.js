// https://www.npmjs.com/package/debounce
(function () {
  function debounce(func, wait) {
    let timeout, args, context, timestamp, result;
    if (null == wait) wait = 100;

    function later() {
      const last = Date.now() - timestamp;

      if (last < wait && last >= 0) {
        timeout = setTimeout(later, wait - last);
      } else {
        timeout = null;
        result = func.apply(context, args);
        context = args = null;
      }
    }

    const debounced = function () {
      context = this;
      args = arguments;
      timestamp = Date.now();
      if (!timeout) timeout = setTimeout(later, wait);

      return result;
    };

    debounced.clear = function () {
      if (timeout) {
        clearTimeout(timeout);
        timeout = null;
      }
    };

    debounced.flush = function () {
      if (timeout) {
        result = func.apply(context, args);
        context = args = null;

        clearTimeout(timeout);
        timeout = null;
      }
    };

    return debounced;
  }

  window.debounce = debounce;
})();
