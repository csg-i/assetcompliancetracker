
$.onDataEvent('matchCount',
    'span.match-count',
    function(v) {
        $(this).html(v);
    });
$.onDataEvent('count',
    'span.count',
    function (v) {
        $(this).html(v);
    });
$.onDataEvent('changeSearchType',
    'span.search-type',
    function (val) {
        $(this).html(val);
    });

$.onDataEvent('hideJumbotron',
    'div.jumbotron',
    function (hide) {
        $(this)[hide ? 'addClass' : 'removeClass']('d-none');
    });

$.onDataEvent('changeSearchAndPlaceHolder',
    'input[name="search"]',
    function (data) {
        var $t = $(this).data(data.filterType, data.value);
        if (data.placeHolder) {
            $t.attr('placeholder', data.placeHolder).val('');
        }
        if (data.skip !== true) {
            $t.doAction('search');
        }
    });
$.onDataEvent('changeExclusions',
    'input[name="search"]',
    function (data) {
        $(this).data('hideProductExclusions', data).doAction('search');
    });
$.onDataEvent('clearResults',
    '#results',
    function() {
        $(this).empty();
        $.triggerDataEvent({
            matchCount: 0,
            count: 0
        });
    });
$.onDataEvent('search',
    '#results',
    function (data) {
        var $results = $(this).showContentLoading('md').messageRemove();
        var url = $results.data('url');
        jay.ajaxPostHtml({
            url: url,
            data: data
        }).done(function(html) {
            var $html = $(html);
            var hData = $html.data();
            $.triggerDataEvent({
                hideJumbotron: hData.empty !== true,
                matchCount: hData.matchCount,
                count: hData.count
            });
            var pageIndex = parseInt(data.pageIndex, 10) || 0;
            $results.hideContentLoading();
            if (pageIndex === 0) {
                $results.html($html);
            } else {
                $results.find('div.list-group').append($html.find('div.list-group-item'));
            }
            $results.trigger('loadcomplete');
            if (hData.empty === true) {
                $results.message('info', 'No search results.  Change your query and try again.');
            } else if (hData.count < hData.matchCount) {

                var newPageIndex = pageIndex + 1;
                var newPlace = newPageIndex * 30 - 10;
                var nth = $results.find('div.list-group-item:eq(' + newPlace + ')')
                    .one('youcanseeme',
                        function() {
                            $(document).data('watches', []);
                            $.triggerDataEvent('search', $.extend({}, data, { pageIndex: newPageIndex }));
                        });
                $(document).data('watches', [nth[0]]);
            }
        }).fail(function() {
            $.triggerDataEvent('hideJumbotron', false);
            $results.hideContentLoading().empty().message('error', jay.failMessage);
        });
    });

$.onAction('changeFilter',
    function (e, data) {
        $.triggerDataEvent('change' + jay.camelToPascal(data.filterType), data.text);        
        $.triggerDataEvent({
            clearResults: true,
            changeSearchAndPlaceHolder: data
        });
    });
$.onAction('changeExclusions',
    function () {
        $.triggerDataEvent({
            clearResults: true,
            changeExclusions: $(this).is(':checked')
        });
    });
$.onAction('scrollTop',
    function (e, data) {
        $('html,body').animate({
                scrollTop: $(data.where).offset().top - 100
            },
            700);
    });
$.onAction('search',
    function (e, data) {
        var values = $(this).getInputValues();
        var all = $.extend({}, values, data);
        var anythingSet = false;
        if (data.lockr === true) {
            Lockr.set('nodesearch', all);
            var types = ['compliance', 'platform', 'securityClass', 'environment'];
            for (var i = 0; i < types.length; i++) {
                var filterType = types[i];
                var value = all[filterType];
                if (value) {
                    anythingSet = true;
                }
            }
        }
        if (all.search && all.search.length || all.searchType === 'Mine' || anythingSet === true) {
            $.triggerDataEvent('search', all);
        } else {
            $.triggerDataEvent('hideJumbotron', false);
            $(this).trigger('focus');
        }
    });

$.fn.wireUpMultiSelect = function(){
    var msOptions = {
        numberDisplayed:0,
        buttonContainer: '<div class="btn-group btn-group-lg mr-1 mt-1"/>',
        selectAllNumber: false
    };
    return this.each(function () {
        var $t = $(this);
        var d = $t.data();
        var options = $.extend({}, msOptions, {
            nonSelectedText: 'All ' +  d.text,
            allSelectedText: 'All ' +  d.text,
            nSelectedText: d.text,
            onChange: function () {
               var value = $t.val();
               $t.doAction('changeFilter', $.extend({}, d, { value: value}));
            }
        });
        var colors = {};
        $('option', $t).each(function () {
            var $o = $(this);
            var od = $o.data();
            if (od.color){
                colors[$o.attr('value')] = od.color;
            } 
        });
        $t.multiselect(options);
        var ms = $t.data('multiselect');
        for(var color in colors){
            if (colors.hasOwnProperty(color)) {
                ms.$container.find(':checkbox[value="'+color+'"]').closest('div.form-check').find('label').css('color', colors[color]);
            }
        }
    });
};
$.onAction('wireUpSearch',
    function(e, data) {
        var $t = $(this)
            .on('focus',
                function() {
                    $(this)
                        .one('blur',
                            function() {
                                $(this).off('keypress');
                            })
                        .on('keypress',
                            function(e2) {
                                if (e2.which === 13) {
                                    $(this).blur();
                                }
                            });
                });
        $t.val('');
       
        if (data.lockr === true) {
            var last = Lockr.get('nodesearch');
            if (last) {
                if (last.hideProductExclusions === true) {
                    $('#hideProductExclusions').attr('checked', 'checked');
                }
                $t.data('hideProductExclusions', last.hideProductExclusions === true);
                var anythingSet = false;
                var filterType = 'searchType';
                var value = last[filterType];
                if (value) {
                    var $it = $('button.action-change-filter[data-filter-type="'+filterType+'"][data-value="'+value+'"]');
                    var itsData = $.extend({}, $it.data(), { skip: true });
                    $it.doAction('changeFilter', itsData);
                   
                }
                var types = ['compliance', 'platform', 'securityClass', 'environment'];
                for (var i = 0; i < types.length; i++) {
                    filterType = types[i];
                    value = last[filterType];
                    var $select = $('select[multiple][data-filter-type="'+filterType+'"]');
                    var d = $select.data();
                    if (value) {
                        if ($.isArray(value)){
                            for(var j = 0; j < value.length; j++){
                                $select.find('option[value="'+value[j]+'"]').attr('selected','selected');
                            } 
                        }  else{
                            $select.find('option[value="'+value+'"]').attr('selected','selected');
                        }
                        anythingSet = true;
                    }
                    $select.doAction('changeFilter', $.extend({}, d, { value: value, skip: true}));
                    $select.wireUpMultiSelect();
                }
                $t.wait(200,
                    function() {
                        if (last.search) {
                            $t.val(last.search).doAction('search');
                        } else if (last.searchType === 'Mine' || anythingSet === true) {
                            $t.doAction('search');
                        } else {
                            $t.trigger('focus');
                        }
                    });
            }
            else {
                $('select[multiple]').wireUpMultiSelect();
            }
        } else {
            $('select[multiple]').wireUpMultiSelect();
            $t.trigger('focus');
        }
    });

$.fn.visibleHeight = function() {
    var scrollTop = $(window).scrollTop();
    var scrollBot = scrollTop + $(window).height();
    var elTop = this.offset().top;
    var elBottom = elTop + this.outerHeight();
    var visibleTop = elTop < scrollTop ? scrollTop : elTop;
    var visibleBottom = elBottom > scrollBot ? scrollBot : elBottom;
    return visibleBottom - visibleTop;
};

$(function() {
    $(window).on('scroll resize', function () {
        var watches = $(document).data('watches') || [];
        _.each(watches, function (el) {
            var $el = $(el);
            if ($el.length) {
                if ($el.visibleHeight() > 0) {
                    $el.trigger('youcanseeme');
                }
            }
        });
    });
});