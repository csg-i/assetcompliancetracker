var filters = {
    justifications: [],
    justificationHide: true,
    text: ''
};
$.onDataEvent('showMessageAboutHidden',
    '#messageArea',
    function (bool) {
        if (bool === true) {
            $(this).message('success',
                'Justification was assigned, but the filter caused it to be hidden.',
                3500);
        }
    });

$.onDataEvent('showMessage',
    '#messageArea',
    function (data) {
            $(this).message(data.type,
                data.msg,
                10000);
        
    });

$.onDataEvent('assignJustificationsAfterBulkAdd',
    '#justifications',
    function (packages) {
        $(this).find('div.list-group-item:not(.example)').each(function() {
            var data = $(this).getJustificationData();
            $(_.filter(packages,
                        function(a) {
                            return a.data('justificationId') === data.justificationId;
                        })
                )
                .addColorBadge(data.color, data.uniqueId);
        });
    });
$.onDataEvent('assignJustificationsAtLoadTime',
    '#packages',
    function(justifications) {
        var $t = $(this);
        justifications.each(function () {
            var data = $(this).getJustificationData();
            $('li.list-group-item[data-justification-id="' + data.justificationId + '"]', $t).addColorBadge(data.color, data.uniqueId);
        });

    });
$.onDataEvent('selectPackages',
    '#packages',
    function (all) {
        var $t = $(this);
        if (all === true) {
            $('li.list-group-item:not(.example):not(:hidden)', $t).addClass('active');
        } else {
            $('li.list-group-item:not(.example)', $t).removeClass('active');
        }
    });
$.onDataEvent('addPackage',
    '#packages',
    function (toAdd) {
        var $t = $(this);
        $.triggerDataEvent('togglePackageExampleText',
            {
                hide: true,
                fn: function() {
                    toAdd.appendTo($t).wireUpPackages();
                    $.triggerDataEvent('showControl', true);
                }
            });
    });
$.onDataEvent('togglePackageExampleText',
    '#packages li.list-group-item.example',
    function(data) {
        $(this)[data.hide === true ? 'slideUp' : 'slideDown']('fast', function() {
            if (data.fn && $.isFunction(data.fn)) {
                data.fn.apply(this, arguments);
            }
        });
    });
$.onDataEvent('removePackage',
    '#packages',
    function (it) {
        var $t = $(this);
        it.draggable('destroy').removeUniqueId().remove();
        if ($('li.list-group-item:not(.example)', $t).length === 0) {
            $.triggerDataEvent('togglePackageExampleText',
                {
                    hide: false,
                    fn: function () {
                        $.triggerDataEvent('showControl', false);
                    }
                });
        }
    });
$.onDataEvent('clearPackages',
    '#packages',
    function () {
        var $t = $(this);
        $('li.list-group-item:not(.example)', $t).draggable('destroy').removeUniqueId().remove();
        $.triggerDataEvent('togglePackageExampleText',
            {
                hide: false,
                fn: function() {
                    $.triggerDataEvent('showControl', false);
                }
            });
    });
$.onDataEvent('showControl',
    '#control',
    function(bool) {
        $(this)[bool === true ? 'slideDown':'slideUp']();
    });
$.onDataEvent('toggleJustificationExampleText',
    '#justifications div.example',
    function(data) {
        $(this)[data.hide === true ? 'fadeOut' : 'fadeIn']('fast', function () {
            if (data.fn && $.isFunction(data.fn)) {
                data.fn.apply(this, arguments);
            }
        });
    });
$.onDataEvent('addJustification',
    '#justifications',
    function (toAdd) {
        var $t = $(this);
        var data = toAdd.data();
        var url = null;
        if (!data.color) {
            url = $('button.action-change-color', toAdd).data('url');
        }
        $.triggerDataEvent('toggleJustificationExampleText',
            {
                hide: true,
                fn: function() {
                    toAdd.setBorderColor(data.color, url).appendTo($t).wireUpJustifications();
                }
            });
    });
$.onDataEvent('removeJustification',
    '#justifications',
    function (toRemove) {
        toRemove.droppable('destroy').removeUniqueId().remove();
        if ($('div.list-group-item:not(.example)', this).length === 0) {
            $.triggerDataEvent('toggleJustificationExampleText',
                {
                    hide: false
                });
        }
    });
$.onDataEvent('unlinkJustification',
    '#packages',
    function (id) {
        $('li.list-group-item[data-justification="' + id + '"]', this).each(function () {
            $(this).removeColorBadge();
        });
    });
$.onDataEvent('filterLabelChange',
    'em.filter-label',
    function (data) {
        var $t = $(this);
        if (data.type !== 'text') {
            $t.data('justificationType', data.type);
        }
        var html = 'No Filter';
        var td = $t.data();
        switch (td.justificationType) {
        case 'hide':
            html = 'Hidden Filter';
            break;
        case 'only':
            html = 'Show Only Filter';
            break;
        case 'all':
            html = 'No Filter';
            break;
        case 'not':
            html = 'Unjustified Filter';
            break;
        }

        if (data.text !== '') {
            if (td.justificationType === 'all') {
                $t.html('Text Filter');
            } else if (td.text !== true) {
                $t.html(html + ' + Text Filter');
            }
            $t.data('text', true);
        } else {
            $t.html(html);
            $t.data('text', false);
        }
    });
$.onAction('closePackageExample',
    function() {
        $.triggerDataEvent('togglePackageExampleText', { hide: true });
    });
$.onAction('bootstrapPackages',
    function (e, data) {
        var $t = $(this);
        var it = $('li.list-group-item:not(.example)', $t).wireUpPackages();
        $.triggerDataEvent('showControl', it.length > 0);

        $.triggerDataEvent('wizardBeforeStepFunctionChange',
            {
                fn: function (widgetData, success) {
                    if ($('li.list-group-item[data-justification-id=""]', $t).length) {
                        var $area = $(this).hideContentLoading();
                        jay.modal({
                            title: 'Confirm',
                            content: $('<p/>').html('There are still some ' +
                                data.name +
                                ' that have not been justified. Do you want to continue to the next screen, or stay here?  Click "Unjustified" to quickly find them.'),
                            size: 'small',
                            buttons: [
                                {
                                    label: 'Continue',
                                    cssClass: 'btn-danger',
                                    close:true,
                                    onClick: function () {
                                        $area.showContentLoading();
                                        success.apply($t);
                                    }

                                },
                                {
                                    label: 'Stay Here',
                                    close: true
                                }
                            ]
                        });
                    } else {
                        success.apply($t);
                    }
                }
            });
    });
$.onAction('select',
    function (e, data) {
        $.triggerDataEvent('selectPackages', data.type === 'all');
    });
$.onAction('bootstrapTrash',
    function() {
        $(this).wireUpTrash();
    });
$.onAction('filter',
    function (e, data) {
        var $this = $(this);
        var $group = $this.closest('div.list-group-item');
        var id = $group.attr('id');
        switch (data.type) {
        case 'hide':
            if (filters.justifications.indexOf(id) < 0) {
                filters.justifications.push(id);
            }
            filters.justificationHide = true;
            filters.text = '';
            break;
        case 'only':
            filters.justifications = [id];
            filters.justificationHide = false;
            filters.text = '';
            break;
        case 'all':
            filters.justifications = [];
            filters.justificationHide = true;
            filters.text = '';
            break;
        case 'not':
            filters.justifications = [''];
            filters.justificationHide = false;
            filters.text = '';
            break;
        case 'text':
            filters.text = $(this).val().toLowerCase();
            break;
        }
        $.triggerDataEvent('filterLabelChange', { type: data.type, text: filters.text });
        var dataJustifications = _.map(filters.justifications,
            function (i) {
                return $('#packages li.list-group-item:not(.example)').dataFilter(['justification', i]);
            });


        $('#packages li.list-group-item:not(.example)')[filters.justificationHide === true ? 'show' : 'hide']();
        _.each(dataJustifications,
            function (i) {
                $(i)[filters.justificationHide === true ? 'hide' : 'show']();
            });

        if (filters.text && filters.text.length) {
            $('#packages li.list-group-item:not(:hidden)').hide().dataFilter(['text', filters.text, '*=']).show();            
        } else {
            $('#searchFilter').val('');
        }
        $('#packages li.list-group-item:hidden').removeClass('active');
    });
$.onAction('changeColor',
    function (e, data) {
        var $item = $(this).closest('div.list-group-item');
        var doIt = function (color) {
            $(this).setBorderColor(color, data.url);                
        };
        if (data.type === 'choose') {
            var currentColor = jay.rgbToHex($item.css('border-left-color'));
            var $ct = $($('#colorChooserTemplate').html());
            $ct.find(':input[type="color"]').val(currentColor);
            jay.modal({
                title: 'Pick a color',
                content: $ct,
                size: 'small',
                buttons: [
                    {
                        label: 'Cancel',
                        close: true
                    },
                    {
                        label: 'Choose',
                        close:true,
                        onClick: function (e, content) {
                            var formData = content.getInputValues();
                            doIt.apply($item, [formData.color]);
                        }
                    }
                ]
            });
        } else {
            var color = $.generateColor();
            doIt.apply($item, [color]);
        }
    });
$.onAction('deleteJustification',
    function (e, data) {
        var $t = $(this).closest('div.list-group-item');
        var $content = $('<div/>').html(data.message);
        jay.modal({
            size: 'medium',
            content: $content,
            title: 'Delete Justification',
            buttons: [
                {
                    label: 'Cancel',
                    close: true
                },
                {
                    label: 'Delete',
                    close: true,
                    cssClass: 'btn-danger',
                    onClick: function () {
                        jay.ajax({
                            url: data.url
                        });
                        
                        var id = $t.attr('id');

                        $.triggerDataEvent({
                            unlinkJustification: id,
                            removeJustification: $t
                        });
                    }
                }
            ]
        });
    });
$.onAction('readJustification',
    function() {
        var $t = $(this).closest('div.list-group-item');
        var html = $t.find('span.text-justify').html();
        jay.modal({
            title: 'Read',
            content: $('<div/>').html(html),
            buttons: [
                {
                    label: 'Close',
                    close:true
                }
            ]
        });
    });
$.onAction('editJustification',
    function (e, data) {
        var $t = $(this).closest('div.list-group-item');
        jay.modal({
            size: 'medium',
            partialUrl: data.url,
            title: 'Edit Justification',
            static: true,
            open: function () {
                $('textarea', this).selectRange();
            },
            buttons: [
                {
                    label: 'Cancel',
                    close: true
                },
                {
                    label: 'Update',
                    onClick: function (e, content, mw) {
                        mw.showContentLoading();
                        var postData = content.getInputValues();
                       jay.ajax({
                            url: data.saveUrl,
                            data: postData
                        }).done(function () {
                            $t.find('span.text-justify').html(postData.text.newLineHtmlBreak());
                            mw.hideContentLoading().modal('hide');
                        }).error(function (msg) {
                            mw.hideContentLoading();
                            content.message('error', msg);
                        }).fail(function () {
                            mw.hideContentLoading();
                            content.message('error', jay.failMessage);
                        });
                    }
                }
            ]
        });
    });
$.onAction('populateJustifications',
    function (e, data) {
        var $t = $(this).showContentLoading('md');
        jay.ajaxPostHtml({ url: data.url })
            .done(function (html) {
                $t.hideContentLoading();
                var $html = $(html);
                if ($html.data('empty') !== true) {
                    $html.children().each(function() {
                        $.triggerDataEvent('addJustification', $(this));
                    });
                    $t.wait(500,
                        function() {
                            $.triggerDataEvent('assignJustificationsAtLoadTime',
                                $('div.list-group-item:not(div.example)', $(this)));
                        });
                }
            }).fail(function() {
                $t.hideContentLoading().message('error', jay.failMessage);
            });
    });
$.onAction('newJustification',
    function (e, data) {
        jay.modal({
            size: 'medium',
            partialUrl: data.url,
            title: 'Enter a new Justification',
            static: true,
            open: function () {
                $('textarea', this).val('').focus(); //setting val to blank to fix IEBug with placeholder
            },
            buttons: [
                {
                    label: 'Cancel',
                    close: true
                },
                {
                    label: 'Add',
                    onClick: function (e, content, mw) {
                        mw.showContentLoading();
                        jay.ajax({
                            url: data.saveUrl,
                            data: $(':input', content).getInputValues()
                        }).done(function(json) {
                            jay.ajaxPostHtml({
                                url: json.url
                            }).done(function(html) {
                                $.triggerDataEvent('addJustification', $(html));
                                mw.hideContentLoading().modal('hide');
                            }).fail(function() {
                                mw.hideContentLoading();
                                content.message('error', jay.failMessage);
                            });
                        }).error(function(msg) {
                            mw.hideContentLoading();
                            content.message('error', msg);                            
                        }).fail(function () {
                            mw.hideContentLoading();
                            content.message('error', jay.failMessage);
                        });
                    }
                }
            ]
        });
    });
$.onAction('newPackage',
    function (e, data) {
        jay.modal({
            size: 'medium',
            partialUrl: data.url,
            title: 'Enter a new ' + data.type,
            static: true,
            open: function () {
               $(':input:text:first', this).focus();
                $('select[multiple]', this).each(function () {
                    var $ms = $(this);
                    var text = $ms.closest('div.form-group').find('label').hide().text();
                    $(this).multiselect({
                        buttonContainer: $('<div/>').addClass('btn-group').html($('<button/>').addClass('btn btn-outline-secondary').html(text))
                    });
                });
            },
            buttons: [
                {
                    label: 'Cancel',
                    close: true
                },
                {
                    label: 'Add',
                    onClick: function (e, content, mw) {
                        var postData = $(':input', content).getInputValues();
                        mw.showContentLoading();
                        jay.ajax({
                            url: data.saveUrl,
                            data: postData
                        }).done(function (json) {
                            jay.ajaxPostHtml({
                                url: json.url
                            }).done(function (html) {
                                mw.hideContentLoading().modal('hide');
                                $.triggerDataEvent('addPackage', $(html));
                            }).fail(function () {
                                mw.hideContentLoading();
                                content.message('error', jay.failMessage);
                            });
                        }).error(function (msg) {
                            mw.hideContentLoading();
                            content.message('info', msg);
                        }).fail(function () {
                            mw.hideContentLoading();
                            content.message('error', jay.failMessage);
                        });
                    }
                }
            ]
        });
    });
$.onAction('bulkAdd',
    function (e, data) {
        jay.modal({
            size: 'xl',
            partialUrl: data.url,
            title: 'Bulk enter ' + data.type,
            static: true,
            open: function () {
                $('textarea', this).val('').focus(); //setting val to blank to fix IEBug with placeholder
            },
            buttons: [
                {
                    label: 'Cancel',
                    close: true
                },
                {
                    label: 'Add',
                    onClick: function (e, content, mw) {
                        var postData = $(':input', content).getInputValues();
                        var perLine = _.without(postData.bulktext.split('\n'), '');
                        if (postData.addType !== 'NamesOnly') {
                            var splitOnTab = _.map(perLine,
                                function (line) {
                                    var split = line.split('\t');
                                    return {
                                        name: split[0],
                                        other: split[1] || ''
                                    };
                                });
                            postData.names = _.pluck(splitOnTab, 'name');
                            postData.others = _.pluck(splitOnTab, 'other');
                        } else {
                            postData.names = _.uniq(perLine);
                        }
                        delete postData.bulktext;
                        mw.showContentLoading();
                        jay.ajax({
                            url: data.saveUrl,
                            data: postData
                        }).done(function (json) {
                            if (json.justifications === true) {
                                mw.hideContentLoading().modal('hide');
                                $.triggerDataEvent('wizardStepChange', 0);
                                setTimeout(function () {
                                    if (json.skipped && json.skipped.length) {
                                        $.triggerDataEvent('showMessage', { type: 'info', msg: 'Some of the ' + json.type + ' entered were already on the Operating System Specification or this Application Specification.  ' + json.skipped });
                                    }
                                }, 1000);
                            } else {
                                if (json.skipped && json.skipped.length) {
                                    $.triggerDataEvent('showMessage', { type: 'info', msg: 'Some of the ' + json.type + ' entered were already on the Operating System Specification or this Application Specification.  ' + json.skipped });
                                }
                                jay.ajaxPostHtml({
                                    url: json.url
                                }).done(function(html) {
                                    $.triggerDataEvent('clearPackages');
                                    var all = [];
                                    $(html).wait(100,
                                        function() {
                                            $(this).children().each(function() {
                                                var $i = $(this);
                                                $.triggerDataEvent('addPackage', $i);
                                                all.push($i);
                                            });
                                        }).wait(500, function() {
                                            $.triggerDataEvent('assignJustificationsAfterBulkAdd', all);
                                        });
                                    mw.hideContentLoading().modal('hide');
                                }).fail(function() {
                                    mw.hideContentLoading();
                                    content.message('error', jay.failMessage);
                                });
                            }
                        }).error(function (msg) {
                            mw.hideContentLoading();
                            content.message('error', msg);
                        }).fail(function() {
                            mw.hideContentLoading();
                            content.message('error', jay.failMessage);
                        });
                    }
                }
            ]
        });
    });
$.onAction('wireUpPackageSearch',
    function () {
        $(this).on('focus',
            function () {
                $(this)
                    .one('blur',
                        function () {
                            $(this).off('keyup');
                        })
                    .on('keyup',
                        function () {
                            $(this).doAction('filter');
                        });
            });
    });
$.onAction('cleanDuplicates',
    function(e, data) {
        jay.modal({
            title: 'Confirm',
            content: $('<div/>').html('<p>This will remove any ' +
                data.type +
                ' that is already on the OS spec, but will leave justifications alone.  Proceed?'),
            size: 'small',
            buttons: [
                {
                    label: 'No, Cancel',
                    close: true
                },
                {
                    label: 'Yes, Proceed',
                    onClick: function(evt, content, mw) {
                        mw.showContentLoading();
                        jay.ajax({ url: data.url })
                            .done(function () {
                                mw.hideContentLoading().modal('hide');
                                $.triggerDataEvent('wizardStepChange', 0);
                            }).error(function (err) {
                                mw.hideContentLoading();
                                content.message('error', err);
                            }).fail(function () {
                                mw.hideContentLoading();
                                content.message('error', jay.failMessage);
                            });
                    }
                }
            ]
        });
    });
$.extend({
    generateColor: function () {
        return '#' + (Math.random() * 0xFFFFFF << 0).toString(16);
    }
});
$.fn.setBorderColor = function (color, url) {
    return this.each(function () {
        color = color || $.generateColor();
        var $t = $(this);
        if (url) {
            jay.ajax({
                url: url,
                data: {
                    color: color
                }
            }).done(function() {
                $t.css('border-left-color', color);
                var id = $t.attr('id');
                $('#packages li.list-group-item[data-justification="' + id + '"]').each(function () {
                    $(this).addColorBadge(color, id);
                });
            }).fail(function() {
                $.triggerDataEvent('showMessage', { type: 'error', msg: 'The color was not saved correctly.' });
            });
        } else {
            $t.css('border-left-color', color);
        }
    });
};
$.fn.removeColorBadge = function () {
    return this.each(function () {
        $(this).attr('data-justification', '').find('span.badge').remove();
    });
};
$.fn.addColorBadge = function (color, justificationId) {
    return this.removeColorBadge().each(function () {
        $(this).append($('<span/>').addClass('badge badge-pill float-right').css('background-color', color).html('&nbsp;'))
            .attr('data-justification', justificationId);
    });
};
$.fn.getJustificationData = function() {
    return {
        justificationId: this.data('justificationId'),
        color: this.css('border-left-color'),
        uniqueId: this.attr('id')
    };
};
$.fn.wireUpJustifications = function () {
    return this
        .droppable({
            accept: 'li.list-group-item',
            classes: {
                'ui-droppable-active': 'outline',
                'ui-droppable-hover': 'active'
            }
        })
        .on('drop',
            function (evt, ui) {
                var $drop = $(this);
                var dropData = $drop.getJustificationData();
                var $drag = $(ui.draggable).addColorBadge(dropData.color, dropData.uniqueId).attr('data-justification-id', dropData.justificationId).removeClass('active');
                var dragData = $drag.data();
                jay.ajax({
                    url: dragData.assignUrl,
                    data: {
                        justificationId: dropData.justificationId
                    }
                });
                (dragData.others || $([])).each(function () {
                    var url = $(this).addColorBadge(dropData.color, dropData.uniqueId).attr('data-justification-id',dropData.justificationId).removeClass('active').data('assignUrl');
                    jay.ajax({
                        url: url,
                        data: {
                            justificationId: dropData.justificationId
                        }
                    });
                });
                $drag.removeData('others');
                $drop.doAction('filter');
                $.triggerDataEvent('showMessageAboutHidden', $drag.is(':hidden'));
            })
        .on('dblclick',
            function () {
                $(this).doAction('readJustification');
            })
        .uniqueId();
};
$.fn.wireUpPackages = function () {
    return this
        .on('dblclick',
            function () {
                var $t = $(this);
                var data = $t.data();
                jay.modal({
                    partialUrl: data.editUrl,
                    title:'Edit',
                    size: 'medium',
                    open: function () {
                        $(':input:text:first', this).selectRange();
                        $('select[multiple]', this).each(function () {
                            var $ms = $(this);
                            var text = $ms.closest('div.form-group').find('label').hide().text();
                            $(this).multiselect({
                                buttonContainer: $('<div/>').addClass('btn-group').html($('<button/>').addClass('btn btn-outline-secondary').html(text))
                            }); 
                        });
                    },
                    buttons: [
                        {
                            label: 'Cancel',
                            close: true
                        }, {
                            label: 'Save',
                            onClick: function (e, c, mw) {
                                var post = $(':input', c).getInputValues();
                                mw.showContentLoading();
                                jay.ajax({
                                    url: data.saveUrl,
                                    data: post
                                }).done(function (json) {

                                    jay.ajaxPostHtml({
                                        url: json.url
                                    }).done(function(html) {
                                        mw.hideContentLoading().modal('hide');
                                        $t.html($(html).html());
                                        $.triggerDataEvent('assignJustificationsAfterBulkAdd', [$t]);
                                    }).fail(function() {
                                        mw.hideContentLoading();
                                        c.message('error', jay.failMessage);
                                    });
                                }).error(function(m) {
                                    mw.hideContentLoading();
                                    c.message('error', m);
                                }).fail(function() {
                                    mw.hideContentLoading();
                                    c.message('error', jay.failMessage);
                                });
                            }
                        }
                    ]
                });
            })
        .on('click',
            function (evt) {
                var $t = $(this);
                if (evt.ctrlKey === false) {
                    $t.siblings('li.list-group-item.active').removeClass('active');
                }
                var hasActive = $t.hasClass('active');
                $t[hasActive ? 'removeClass' : 'addClass']('active');
            })
        .draggable({
            handle: 'span.handle',
            cursor: 'move',
            cursorAt: {
                top: 18,
                left: 128
            },
            helper: function () {
                var $t = $(this);
                var allActive = $t.siblings('li.list-group-item.active');
        
                var clone = $t.clone();
                clone.removeClass('active').find('span.badge').remove().end().find('span.text').css({
                    width: '200px',
                    overflow:'hidden'
                });

                $t.data('others', allActive);
                if (allActive.length > 0) {
                    clone.prepend($('<span/>').addClass('badge badge-pill float-right').append('+').append(allActive.length));
                }

                return clone.css({
                    display: 'block',
                    width: '256px',
                    height: '36px',
                    overflow:'hidden',
                    'z-index': 4000
                });
            },
            opacity: 0.80
        }).uniqueId().each(function () {
            var $t = $(this);
            $t.attr('data-text', $('span.text',$t).html().toLowerCase());
        });
};
$.fn.wireUpTrash = function () {
    return this.droppable({
        accept: 'li.list-group-item',
        classes: {
            'ui-droppable-active': 'active',
            'ui-droppable-hover': 'hover'
        }
    }).on('drop',
        function(evt, ui) {
            var $drag = $(ui.draggable);
            var $others = $drag.data('others') || $([]);
            jay.ajax({
                url: $drag.data('deleteUrl')
            }).done(function() {
                $.triggerDataEvent('removePackage', $drag);
            });
            $others.each(function() {
                var $t = $(this);
                jay.ajax({
                    url: $t.data('deleteUrl')

                }).done(function() {
                    $.triggerDataEvent('removePackage', $t);
                });

            });
        });
};
