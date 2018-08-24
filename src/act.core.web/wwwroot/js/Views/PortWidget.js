$.onDataEvent('hidePortExample',
    '#ports div.example',
    function(hidden) {
        $(this)[hidden === true ? 'slideUp' : 'slideDown']('fast');
    });
$.onDataEvent('hidePortExample',
    '#ports div.example',
    function (hidden) {
        $(this)[hidden === true ? 'slideUp' : 'slideDown']('fast');
    });
$.onDataEvent('hideExternalCheckbox',
    'div.checkbox.external',
    function (hidden) {
        $(this)[hidden === true ? 'slideUp' : 'slideDown']('fast');
    });
$.onDataEvent('addOrReplacePort',
    '#ports',
    function (data) {
        var $t = $(this);
        jay.ajaxPostHtml({
            url: data.url
        }).done(function (html) {
            if (data.edit === true) {
                data.replace
                    .wrap('<div class="wrap-temp"/>')
                    .closest('div.wrap-temp')
                    .html(html)
                    .find('div.list-group-item')
                    .unwrap('div.wrap-temp');
            } else {
                $t.append(html);
            }
            $.triggerDataEvent('hidePortExample', true);
        }).fail(function() {
            $t.message('error', jay.failMessage);
        });
    });
$.onAction('viewToggle',
    function() {
        var $icon = $(this).find('i.fa');
        if ($icon.hasClass('fa-eye')) {
            $icon.removeClass('fa-eye').addClass('fa-eye-slash').closest('div.list-group-item').addClass('all');
        } else {
            $icon.removeClass('fa-eye-slash').addClass('fa-eye').closest('div.list-group-item').removeClass('all');
        }
    });
$.onAction('populatePorts',
    function(e, data) {
        var $t = $(this).messageRemove().showContentLoading();
        jay.ajaxPostHtml({
                url: data.url
            })
            .done(function(html) {
                $t.hideContentLoading();
                var $html = $(html);
                var data = $html.data();
                if (data.empty !== true) {
                    $t.append($html.html());
                    $.triggerDataEvent('hidePortExample', true);
                }
            }).fail(function() {
                $t.hideContentLoading().message('error', jay.failMessage);
            });
    });
$.onAction('deletePort',
    function (e, data) {
        var $t = $(this).closest('.list-group-item');
        jay.modal({
            title: 'Confirm Delete',
            content: $('<div>Are you sure you want to delete this port justification</div>'),
            size: 'small',
            buttons: [
                {
                    label: 'No, Cancel',
                    close: true
                },
                {
                    label: 'Yes, Delete',
                    cssClass: 'btn-danger',
                    onClick: function(evt, content, mw) {
                        content.messageRemove();
                        mw.showContentLoading();
                        jay.ajax({
                            url: data.url
                        }).done(function () {                           
                            mw.hideContentLoading().modal('hide');
                            $t.slideUp('fast',
                                function() {
                                    $(this).remove();
                                });
                        }).fail(function() {
                            mw.hideContentLoading();
                            content.message('error', jay.failMessage);
                        });
                    }
                }
            ]
        });
    });
$.onAction('addEditPort',
    function (evt, data) {
        var $t = $(this).closest('div.list-group-item');

        var saveFunc = function(e, content, mw, data, addAnother) {
            content.clearJsonValidation();
            mw.showContentLoading();
            var postData = content.messageRemove().getInputValues();
            jay.ajax({
                url: data.validateUrl,
                data: postData
            }).fail(function() {
                mw.hideContentLoading();
                content.message('error', jay.failMessage);
            }).error(function(error) {
                mw.hideContentLoading();
                content.applyJsonValidation(error);                 
            }).done(function() {
                jay.ajax({
                    url: data.saveUrl,
                    data: postData
                }).done(function(json) {
                    $.triggerDataEvent('addOrReplacePort',
                        {
                            url: json.url,
                            edit: data.edit,
                            replace: $t
                        });
                    mw.hideContentLoading();
                    if (addAnother === true && data.edit !== true) {
                        $(':input', content).clearForm();
                        $(':input:radio:first').trigger('click');
                        $('textarea:first', content).focus();
                    } else {
                        mw.modal('hide');
                    }
                }).error(function(error) {
                    mw.hideContentLoading();
                    content.message('error', error);
                }).fail(function() {
                    mw.hideContentLoading();
                    content.message('error', jay.failMessage);
                });
            });
        };

        var buttons = [
            {
                label: 'Close',
                close: true
            },
            {
                label: 'Save',
                cssClass: 'btn-success',
                onClick: function(e, content, mw) {
                    saveFunc.apply(this, [e, content, mw, data, false]);
                }
            }
        ];
        if (data.edit !== true) {
            buttons.push({
                label: 'Save &amp; Add Another',
                cssClass: 'btn-primary',
                onClick: function (e, content, mw) {
                    saveFunc.apply(this, [e, content, mw, data, true]);
                }
            });
        }
        jay.modal({
            title: data.title,
            partialUrl: data.url,
            open: function () {
                if (data.edit === true) {
                    $('textarea', this).selectRange();
                } else {
                    $('textarea', this).val('').first().focus();
                }
            },
            size: 'large',
            buttons: buttons,
            static: true
        });
    });
$.onAction('bulkAddPorts',
    function(e, data) {
        jay.modal({
            title: data.title,
            partialUrl: data.url,
            open: function () {
                if (data.edit === true) {
                    $('textarea', this).selectRange();
                } else {
                    $('textarea', this).val('').first().focus();
                }
            },
            size: 'xl',
            static: true,
            buttons:[
                {
                    label: 'Cancel',
                    close:true
                },
                {
                    label: 'Validate',
                    cssClass: 'btn-success',
                    onClick: function(evt, content, mw) {
                        var paste = $('textarea[name="paste"]', content).getInputValues().paste;
                        content.messageRemove();
                        if (paste) {
                            var out = [];
                            var rows = paste.split('\n');
                            var rowCount = rows.length;
                            for (var i = 0; i < rowCount; i++) {
                                var tabs = rows[i].split('\t');
                                if (tabs.length >= 2) {
                                    out.push({
                                        ports: tabs[0].replace(/\s/gi, '').replace(/-/gi, ':').replace(/,/gi, '\n'),
                                        justification: tabs[1]
                                    });
                                }
                            }
                            var outLength = out.length;
                            if (outLength > 0) {
                                var $btn = $(this);
                                var rowMarkup = $('#rowtemplate',content).html();
                                var $validTable = $('#valid', content).slideDown('fast',function() {
                                    $('#pastefromexcel', content).slideUp('fast');
                                    $btn.attr('disabled', 'disabled');
                                    $btn.closest('div.modal-footer').find('button.btn-primary').removeAttr('disabled');
                                }).find('tbody').empty();
                                for (var j = 0; j < outLength; j++) {
                                    var $row = $(rowMarkup).clone();
                                    $row.find('textarea[name="ports"]').val(out[j].ports);
                                    $row.find('textarea[name="justification"]').val(out[j].justification);
                                    $row.find('textarea').on('focus', function () {
                                        $(this).one('blur',
                                            function () {
                                                $(this).closest('tr').validateRow();
                                            });
                                    });
                                    $row.find('select').on('change', function () {
                                        $(this).closest('tr').validateRow();
                                    });
                                    $validTable.append($row);
                                    $row.validateRow();
                                }
                            } else {
                                content.message('error', 'Check your input and try again.');
                            }
                        } else {
                            content.message('error', 'Nothing pasted to validate.');
                        }

                    }
                },
                {
                    label: 'Import',
                    cssClass: 'btn-primary',
                    disabled: true,
                    onClick: function (evt, content, mw) {
                        content.messageRemove();
                        mw.showContentLoading();
                      
                        if ($('#valid', content).find('tbody tr:not(.valid)').length) {
                            content.message('error', 'Fix the errors and try again.');
                            mw.hideContentLoading();
                        } else {
                            var interval = setInterval(function () {
                                    if ($('#valid', content).find('tr.valid').length === 0) {
                                        mw.hideContentLoading();
                                        clearInterval(interval);
                                        mw.modal('hide');
                                    }
                                },
                                200);
                            $('#valid', content).find('tr.valid').each(function () {
                                var $tr = $(this);
                                var postData = $(':input', $tr).getInputValues();
                                var trData = $tr.data();
                                jay.ajax({
                                    url: trData.saveUrl,
                                    data: postData
                                }).done(function(json) {
                                    $.triggerDataEvent('addOrReplacePort',
                                        {
                                            url: json.url,
                                            edit: false
                                        });
                                    $tr.remove();
                                }).fail(function() {
                                    mw.hideContentLoading();
                                    content.message('error', jay.failMessage);
                                    clearInterval(interval);
                                }).error(function(error) {
                                    mw.hideContentLoading();
                                    content.message('error', error);
                                    clearInterval(interval);
                                });

                            });
                         
                        }
                    }
                }
            ]
        });
    });

$.fn.validateRow = function() {
    return this.each(function() {
        var $t = $(this);
        var data = $t.data();
        var postData = $(':input', $t).getInputValues();
        $('div.form-group', $t).find('span.form-text').remove();
        $('.form-control.is-invalid', $t).removeClass('is-invalid');
        $t.removeClass('valid').find('td:last').empty();
        jay.ajax({
            url: data.validateUrl,
            data: postData
        }).fail(function() {
            $t.find('td:last').html($('<i class="fa fa-ban"/>')
                .attr('title', error).wrap('<div class="text-danger"></div>'));
        }).error(function(error) {
            if ($.isPlainObject(error)) {
                for (var key in error) {
                    if (error.hasOwnProperty(key)) {
                        $(':input[name="' + key + '"]', $t)
                            .addClass('is-invalid')
                            .closest('div.form-group')
                            .append($('<span/>')
                                .addClass('form-text text-danger')
                                .html(error[key])
                            );
                    }
                }
                $t.find('td:last').html($('<i class="fa fa-minus-circle text-danger"/>')
                    .attr('title', 'Check the errors and try again.'));
            } else {
                $t.find('td:last').html($('<i class="fa fa-minus-circle text-danger"/>')
                    .attr('title', error));
            }
        }).done(function() {
            $t.addClass('valid').find('td:last').html('<i class="fa fa-check-circle text-success"></i>');
        });
    });
};