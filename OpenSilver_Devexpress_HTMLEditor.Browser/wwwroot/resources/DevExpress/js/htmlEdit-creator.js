(function () {
    window.jsHtmlEditClient = {
        createHtmlEdit: createHtmlEdit,
        setValue: setValue,
        getValue: getValue,
        getReadOnly: getReadOnly,
        setReadOnly: setReadOnly,
        dispose: dispose,
        setHeight: setHeight,
        clearHistory: clearHistory,
    };

    const RETRY_MS = 600;

    const getEditorContainerId = (parentContainerId) =>
        `${parentContainerId}htmlEditor`;
    const getContextMenuId = (parentContainerId) =>
        `${parentContainerId}contextMenu`;
    const getSuggestionListId = (parentContainerId) =>
        `${parentContainerId}suggestionsList`;

    const getInstance = (parentContainerId) => {
        const editorContainerId = getEditorContainerId(parentContainerId);

        if (!window[editorContainerId]) {
            window[editorContainerId] = {};
        }

        return window[editorContainerId];
    };

    const pushOrCreateTimeoutId = (parentContainerId, timeoutId) => {
        const instance = getInstance(parentContainerId);

        if (!instance.timeoutIds) {
            instance.timeoutIds = [];
        }

        instance.timeoutIds.push(timeoutId);
    };

    const getHtmlForEditor = (parentContainerId) => `
    <div id="${getContextMenuId(parentContainerId)}" class="context-menu">
      <ul id="${getSuggestionListId(
        parentContainerId
    )}" class="suggestions-list context-menu-options"></ul>
    </div>

    <div id="${getEditorContainerId(
        parentContainerId
    )}" spellCheck="false"></div>
  `;

    function dispose(parentContainerId) {
        const instance = getInstance(parentContainerId);

        instance.timeoutIds?.forEach((timeOutId) => {
            clearTimeout(timeOutId);
        });
        instance.timeoutIds = [];

        instance.htmlEditSpellChecker?.dispose();
        instance.htmlEditSpellChecker = undefined;

        instance.editor?.dispose();
        $(getEditorContainerId(parentContainerId))?.html("");
        instance.editor = undefined;

        instance.creating = false;
    }

    function createHtmlEdit(onInitialized, onValueChanged, parentContainerId) {
        if (getInstance(parentContainerId).creating) {
            return;
        }

        getInstance(parentContainerId).creating = true;

        endCreate = function () {
            pushOrCreateTimeoutId(
                parentContainerId,
                setTimeout(function () {
                    DevExpress.localization.loadMessages(
                        window.htmlEditLocalizationMessages
                    );
                    DevExpress.localization.locale("sv");

                    const containerElement = document.getElementById(parentContainerId);
                    if (containerElement) {
                        const editorId = getEditorContainerId(parentContainerId);
                        const editorIdSelector = `#${editorId}`;
                        containerElement.innerHTML = getHtmlForEditor(parentContainerId);

                        const headerValues = [false, 1, 2, 3];
                        const maxFileSize = 2097152;
                        const allowedExtensions = [
                            ".bmp",
                            ".dib",
                            ".jpg",
                            ".jpeg",
                            ".png",
                            ".gif",
                        ];

                        getInstance(parentContainerId).editor = $(editorIdSelector)
                            .dxHtmlEditor({
                                customizeModules: function (config) {
                                    config.history = {
                                        userOnly: true,
                                    };
                                },
                                valueType: "html",
                                imageUpload: {
                                    tabs: ["file", "url"],
                                    fileUploadMode: "base64",
                                    fileUploaderOptions: {
                                        accept: allowedExtensions.join(","),
                                        allowedFileExtensions: allowedExtensions,
                                        maxFileSize: maxFileSize,
                                        uploadMode: "useButtons",
                                        onOptionChanged: function (e) {
                                            const files = e.value;
                                            if (e.name === "value" && files.length) {
                                                if (
                                                    !e.value.find((file) =>
                                                        allowedExtensions.some(
                                                            (ext) =>
                                                                file.name.substr(
                                                                    file.name.length - ext.length,
                                                                    ext.length
                                                                ) === ext
                                                        )
                                                    )
                                                ) {
                                                    setTimeout(() => {
                                                        getInstance(parentContainerId).editor?.undo();
                                                        getInstance(
                                                            parentContainerId
                                                        ).editor?.clearHistory();
                                                        alert("Filformatet stöds ej");
                                                    }, 500);

                                                    return;
                                                }

                                                var file = files[0];
                                                if (file.size > maxFileSize) {
                                                    setTimeout(function () {
                                                        getInstance(parentContainerId).editor?.undo();
                                                        getInstance(
                                                            parentContainerId
                                                        ).editor?.clearHistory();
                                                        alert(
                                                            "Limit 2MB."
                                                        );
                                                    }, 500);
                                                }
                                            }
                                        },
                                    },
                                },
                                //clear text formatting when pasting
                                onContentReady: function (e) {
                                    e.component
                                        .getQuillInstance()
                                        .clipboard.addMatcher(Node.ELEMENT_NODE, (_, delta) => {
                                            if (getInstance(parentContainerId).isBackgroundTextChange) return delta;
                                            delta.ops = delta.ops.map((op) => ({
                                                insert: op.insert,
                                            }));

                                            return delta;
                                        });
                                },
                                mediaResizing: {
                                    enabled: true,
                                },
                                onValueChanged({ value }) {
                                    if (onValueChanged) onValueChanged(value);
                                },
                                onInitialized() {
                                    if (onInitialized) onInitialized();

                                    $(editorIdSelector).css("background-color", "white");
                                },
                                toolbar: {
                                    container: null,
                                    items: [
                                        'undo', 'redo', 'separator',
                                        {
                                            name: 'size',
                                            acceptedValues: ['8pt', '10pt', '12pt', '14pt', '18pt', '24pt', '36pt'],
                                        },
                                        {
                                            name: 'font',
                                            acceptedValues: ['Arial', 'Courier New', 'Georgia', 'Impact', 'Lucida Console', 'Tahoma', 'Times New Roman', 'Verdana'],
                                        },
                                        'separator', 'bold', 'italic', 'strike', 'underline', 'separator',
                                        'alignLeft', 'alignCenter', 'alignRight', 'alignJustify', 'separator',
                                        'orderedList', 'bulletList', 'separator',
                                        {
                                            name: 'header',
                                            acceptedValues: [false, 1, 2, 3, 4, 5],
                                        }, 'separator',
                                        'color', 'background', 'separator',
                                        'link', 'image', 'separator',
                                        'clear', 'codeBlock', 'blockquote', 'separator',
                                        'insertTable', 'deleteTable',
                                        'insertRowAbove', 'insertRowBelow', 'deleteRow',
                                        'insertColumnLeft', 'insertColumnRight', 'deleteColumn',
                                    ],
                                    multiline: true,
                                },
                            })
                            .dxHtmlEditor("instance");

                        getInstance(parentContainerId).creating = false;
                    } else {
                        getInstance(parentContainerId).creating = true;
                        endCreate();
                    }
                }, RETRY_MS)
            );
        };

        endCreate();
    }

    const delayUntilEditor = (action, parentContainerId) => {
        if (!getInstance(parentContainerId).editor) {
            pushOrCreateTimeoutId(
                parentContainerId,
                setTimeout(() => {
                    delayUntilEditor(action, parentContainerId);
                }, RETRY_MS)
            );
        } else {
            action();
        }
    };

    function setValue(html, parentContainerId) {
        delayUntilEditor(() => {
            getInstance(parentContainerId).isBackgroundTextChange = true;
            getInstance(parentContainerId).editor.option("value", html);
            getInstance(parentContainerId).isBackgroundTextChange = false;
        }, parentContainerId);
    }

    function getValue(parentContainerId) {
        return clearSpellcheckFormatting(getInstance(parentContainerId).editor?.option("value")) || "";
    }

    function clearSpellcheckFormatting(str) {
        const parser = new DOMParser();
        const html = parser.parseFromString(str, 'text/html');

        const misspelledSpans = html.querySelectorAll('span.misspelled-word');
        for (let i = 0; i < misspelledSpans.length; i++) {
            const span = misspelledSpans[i];
            while (span.firstChild) {
                span.parentNode.insertBefore(span.firstChild, span);
            }
            span.parentNode.removeChild(span);
        }
        return html.body.innerHTML;
    }

    function getReadOnly(parentContainerId) {
        return !!getInstance(parentContainerId).editor?.option("readOnly");
    }

    function setReadOnly(val, parentContainerId) {
        delayUntilEditor(() => {
            getInstance(parentContainerId).editor.option("readOnly", val);
        }, parentContainerId);
    }

    function setHeight(val, parentContainerId) {
        delayUntilEditor(() => {
            getInstance(parentContainerId).editor.option("height", val);
        }, parentContainerId);
    }

    function clearHistory(parentContainerId) {
        delayUntilEditor(() => {
            const editor = getInstance(parentContainerId).editor;

            editor.clearHistory();
            editor.repaint();
        }, parentContainerId);
    }
})();
