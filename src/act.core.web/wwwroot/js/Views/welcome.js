$.onDataEvent('inScope',
    'div.in-scope',
    function(val) {
        $(this).css('height', '250px').highcharts({
            colors: ['#28a745', '#dc3545'],
            title: {
                text: 'In Scope'
            },
            credits: {
                enabled: true,
                href: '/',
                text: 'Status'
            },
            xAxis: {
                categories: ['PCI', 'All']
            },
            tooltip: {
                headerFormat: '<span style="font-size: 10px">{point.key} {series.name}</span><br/>.',
                pointFormat: null,
                pointFormatter: function() {
                    return '<span style="color:' +
                        this.color +
                        '">\u25CF</span><b>' +
                        jay.formatNumber(this.y) +
                        '/' +
                        jay.formatNumber(this.total) +
                        ' (' +
                        jay.formatNumber(jay.precisionRound(this.percentage, 1)) +
                        '%)</b><br/>';
                }
            },
            plotOptions: {
                pie: {
                    cursor: 'pointer'
                },
                series: {
                    events: {
                        click: function(evt) {
                            var pci = evt.point.series.name === 'PCI';
                            Lockr.set('nodesearch',
                                {
                                    compliance: 'Passing',
                                    environment: '',
                                    hideProductExclusions: true,
                                    platform: ['Linux', 'WindowsServer'],
                                    search: '',
                                    searchType: 'Fqdn',
                                    securityClass: pci ? ['A', 'B'] : []
                                });
                            window.location.href = evt.point.series.userOptions.url;
                        }
                    },
                    dataLabels: {
                        enabled: true,
                        distance: 0,
                        style: {
                            color: '#000',
                            fontSize: '10px',
                            textOutline: 'none',
                            fontWeight:'normal'
                        },
                        format: null,
                        formatter: function () {
                            if (this.key === 'Passing') {
                                return '<b style="color:' +
                                    this.color +
                                    '">' +
                                    this.series.name +
                                    ' ' +
                                    this.key +
                                    '</b><br/>' +
                                    jay.formatNumber(this.y) +
                                    ' (' +
                                    jay.formatNumber(jay.precisionRound(this.percentage, 1)) +
                                    '%)<br/>';
                            }

                            return '';
                        },
                        inside: false
                    }
                }
            },
            series: val
        });
    });
$.onDataEvent('failing',
    'div.failing',
    function (val) {
        $(this).css('height', '250px').highcharts({
            colors: ['#dc3545', '#ffc107', '#6c757d'],
            chart: {
                type: 'bar'
            },
            title: {
                text: 'Failing'
            },
            xAxis: {
                categories: ['PCI', 'All']
            },
            credits: {
                enabled: true,
                href: '/',
                text: 'Failures'
            },
            yAxis: {
                min: 0,
                title: {
                    text: '# of Nodes'
                }
            },
            legend: {
                reversed: true
            },
            plotOptions: {
                bar: {
                    cursor: 'pointer'
                },
                series: {
                    events: {
                        click: function (evt) {
                            var pci = evt.point.category === 'PCI';
                            Lockr.set('nodesearch',
                                {
                                    compliance: evt.point.series.userOptions.lockrType,
                                    environment: '',
                                    hideProductExclusions: true,
                                    platform: ['Linux', 'WindowsServer'],
                                    search: '',
                                    searchType: 'Fqdn',
                                    securityClass:  pci ? ['A', 'B'] : []
                                });
                            window.location.href = evt.point.series.userOptions.url;
                        }
                    },
                    stacking: 'percent',
                    dataLabels: {
                        enabled: true,
                        style: {
                            color: '#000',
                            fontSize: '12px',
                            textOutline: 'none'
                        },
                        format: null,
                        formatter: function () {
                            return this.y > 0 ? jay.formatNumber(this.y) : '';
                        },
                        inside: true
                    }
                }
            },
            series: val
        });
    });
$.onDataEvent('outOfScope',
    'div.out-of-scope',
    function (val) {
        $(this).css('height', '250px').highcharts({
            colors: ['#007bff', '#17a2b8', '#f8f9fa', '#6c757d'],
            chart: {
                type: 'bar'
            },
            title: {
                text: 'Out of Scope'
            },
            xAxis: {
                categories: ['PCI', 'All']
            },
            credits: {
                enabled: true,
                href: '/',
                text: 'Out of Scope'
            },
            yAxis: {
                min: 0,
                title: {
                    text: '# of Nodes'
                }
            },
            legend: {
                reversed: true
            },
            plotOptions: {
                bar: {
                    cursor: 'pointer'
                },
                series: {
                    events: {
                        click: function (evt) {
                            if (evt.point.series.userOptions.lockrType) {
                                var pci = evt.point.category === 'PCI';
                                Lockr.set('nodesearch',
                                    {
                                        compliance: '',
                                        environment: '',
                                        hideProductExclusions: true,
                                        platform: evt.point.series.userOptions.lockrType,
                                        search: '',
                                        searchType: 'Fqdn',
                                        securityClass: pci ? ['A', 'B'] : []
                                    });
                            }
                            window.location.href = evt.point.series.userOptions.url;
                        }
                    },
                    stacking: 'percent',
                    dataLabels: {
                        enabled: true,
                        style: {
                            color: '#000',
                            fontSize: '12px',
                            textOutline: 'none'
                        },
                        format: null,
                        formatter: function () {
                            return this.y > 0 ? jay.formatNumber(this.y) : '';
                        },
                        inside: true
                    }
                }
            },
            series: val
        });
    });

$.onDataEvent('product',
    '#product',
    function (json) {
        $(this).css('height', '6000px').highcharts({
            colors: ['#17a2b8', '#007bff'],
            chart: {
                type: 'bar'
            },
            title: {
                text: 'Product AppSpec Spread'
            },
            xAxis: {
                categories: json.categories
            },
            credits: {
                enabled: true,
                href: '/',
                text: 'ACT Product AppSpec Spread'
            },
            yAxis: {
                min: 0
            },
            plotOptions: {
                series: {
                    dataLabels: {
                        enabled: true,
                        style: {
                            color: '#000',
                            fontSize: '12px',
                            textOutline: 'none'
                        },
                        format: null,
                        allowOverlap: true,
                        formatter: function () {
                            return this.y > 0 ? this.y : '';
                        },
                        inside: true
                    }
                }
            },
            series: json.series
        });
    });
$.onDataEvent('os',
    '#os',
    function (json) {
        $(this).css('height', '3000px').highcharts({
            colors: ['#17a2b8', '#007bff'],
            chart: {
                type: 'bar'
            },
            title: {
                text: 'OS Spread'
            },
            xAxis: {
                categories: json.categories
            },
            credits: {
                enabled: true,
                href: '/ScoreCard/Platform',
                text: 'ACT Platform Scorecard'
            },
            yAxis: {
                min: 0
            },
            plotOptions: {
                series: {
                    dataLabels: {
                        enabled: true,
                        style: {
                            color: '#000',
                            fontSize: '12px',
                            textOutline: 'none'
                        },
                        format: null,
                        allowOverlap: true,
                        formatter: function () {
                            return this.y > 0 ? this.y : '';
                        },
                        inside: true
                    }
                }
            },
            series: json.series
        });
    });
$.onDataEvent('drawComplianceOverTime', 
    '#overtime', 
    function (data) {

        var $t = $(this);
        var chart = $t.highcharts();
        if(chart){
            chart.destroy();
        }
        $t.css('height', '400px').showContentLoading();
        jay.ajax({
            url: data.url,
            data: {
                daysBack: data.daysBack,
                pciOnly: data.pciOnly,
                employeeId: data.employeeId,
                filterType: data.filterType
            }
        })
        .done(function(json) {
            var plotBands = _.map(json.weekends, function (w) {
                if (w.length === 2){
                    return {
                        color: '#ffc107',
                        from: w[0],
                        to: w[1],
                        label: {
                            text: 'Weekend'
                        }
                    };
                }
            });

            $t.hideContentLoading().highcharts({
                colors: ['#28a745','#dc3545'],
                chart: {
                    type: 'areaspline'
                },
                title: {
                    text: 'Compliance Over Time (' + (data.pciOnly ? 'PCI)' : 'All)')
                },
                xAxis: {
                    categories: json.categories,
                    title: {
                        text: json.categories[0] + ' - ' + json.categories[json.categories.length - 1]
                    },
                    plotBands: plotBands,
                    min: 0
                },
                credits: {
                    enabled: true,
                    href: '/',
                    text: 'ACT Compliance'
                },
                yAxis: {
                    min: 0,
                    title: {
                        text: 'Count'
                    }
                },
                plotOptions: {
                    series: {
                        dataLabels: {
                            enabled: true
                        }
                    }
                },
                series: json.series
            });

        }).fail(function() {
        $t.hideContentLoading().message('error', jay.failMessage);
    });
});
$.onAction('status',
    function (e, data) {
        var $t = $(this).showContentLoading();
        jay.ajax({ url: data.url })
            .done(function (json) {
                $t.hideContentLoading();
                $.triggerDataEvent(json);
            }).fail(function () {
                $t.hideContentLoading().empty().message('error', jay.failMessage);
            });
    });
$.onAction('spread',
    function (e, data) {
        var $t = $(this).showContentLoading();
        jay.ajax({ url: data.url })
            .done(function (json) {
                $t.hideContentLoading();
                $.triggerDataEvent(json);
            }).fail(function () {
                $t.hideContentLoading().empty().message('error', jay.failMessage);
            });
    });
$.onAction('drawDirectorChart',
    function (e, data) {
        var $t = $(this).css('height', '800px').showContentLoading();
        jay.ajax({
                url: data.url
            })
            .done(function(json) {
                $t.hideContentLoading().highcharts({
                    colors: ['#ffc107', '#dc3545', '#28a745', '#17a2b8', '#007bff'],
                    chart: {
                        type: 'bar'
                    },
                    title: {
                        text: 'Director PCI Score'
                    },
                    xAxis: {
                        categories: json.categories
                    },
                    credits: {
                        enabled: true,
                        href: '/ScoreCard/Director',
                        text: 'ACT Director Score Card'
                    },
                    yAxis: {
                        min: 0,
                        title: {
                            text: 'Current Progess'
                        },
                        labels: {
                            formatter: function() {
                                return this.value + '%';
                            }
                        }
                    },
                    legend: {
                        reversed: true
                    },
                    plotOptions: {
                        bar: {
                            cursor: 'pointer'
                        },
                        series: {
                            events: {
                                click: function (evt) {

                                    Lockr.set('nodesearch',
                                        {
                                            compliance: evt.point.series.chart.series[evt.point.series.index].name.replace(' ', ''),
                                            environment:'',
                                            hideProductExclusions:true,
                                            onactionblur:false,
                                            platform:['Linux', 'WindowsServer'],
                                            search: evt.point.category,
                                            searchType:'Director',
                                            securityClass:['A', 'B']
                                        });
                                    window.location.href = data.searchUrl;
                                }
                            },
                            stacking: 'percent',
                            dataLabels: {
                                enabled: true,
                                style: {
                                    color: '#000',
                                    fontSize: '14px',
                                    textOutline: 'none'
                                },
                                format: null,
                                formatter: function () {
                                    return this.y > 0 ? this.y : '';
                                },
                                inside: true
                            }
                        }
                    },
                    series: json.series
                });

            }).fail(function() {
                $t.hideContentLoading().message('error', jay.failMessage);
            });
    });
$.onAction('drawTopOffendersChart',
    function (e, data) {
        var $t = $(this).css('height', '800px').showContentLoading();
        jay.ajax({
            url: data.url
        })
            .done(function (json) {
                $t.hideContentLoading().highcharts({
                    colors: ['#dc3545'],
                    chart: {
                        type: 'bar'
                    },
                    title: {
                        text: 'Top Offenders Last 24 hours (PCI)'
                    },
                    xAxis: {
                        categories: json.categories
                    },
                    credits: {
                        enabled: true,
                        href: '/',
                        text: 'ACT Compliance Offenders'
                    },
                    yAxis: {
                        min: 0,
                        title: {
                            text: 'Number of Failures'
                        }
                    },
                    legend: {
                        reversed: true
                    },
                    plotOptions: {
                        series: {
                            dataLabels: {
                                enabled: true,
                                style: {
                                    color: '#000',
                                    fontSize: '14px',
                                    textOutline: 'none'
                                },
                                format: null,
                                allowOverlap: true,
                                formatter: function () {
                                    return this.y > 0 ? this.y : '';
                                },
                                inside: true
                            }
                        }
                    },
                    series: json.series
                });

            }).fail(function () {
                $t.hideContentLoading().message('error', jay.failMessage);
            });
    });
$.onAction('drawComplianceOverTime',
    function (e, data) {
       $.triggerDataEvent('drawComplianceOverTime', data);        
    });
$.onAction('afterTypeAhead', 
    function (e, data){
        $(this).doAction('changeComplianceOverTime', { url: data.graphUrl });
    });
$.onAction('changeComplianceOverTime',
    function (e, data) {
        var $form = $(this).closest('div[role="form"]');
        var allData = $.extend({},data, $form.getInputValues());
        $.triggerDataEvent('drawComplianceOverTime', allData);
    });