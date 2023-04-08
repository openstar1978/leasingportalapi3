////angular.module('ui.bootstrap.pagination', [])
////.directive('pagination', function() {
////  return {
////    restrict: 'E',
////    scope: {
////      numPages: '=',
////      currentPage: '=',
////      onSelectPage: '&'
////    },
////    templateUrl: '/templates/pagination.html',
////    replace: true,
////    link: function(scope) {
////      scope.$watch('numPages', function(value) {
////        scope.pages = [];
////        for(var i=1;i<=value;i++) {
////          scope.pages.push(i);
////        }
////        if ( scope.currentPage > value ) {
////          scope.selectPage(value);
////        }
////      });
////      scope.noPrevious = function() {
////        return scope.currentPage === 1;
////      };
////      scope.noNext = function() {
////        return scope.currentPage === scope.numPages;
////      };
////      scope.isActive = function(page) {
////        return scope.currentPage === page;
////      };

////      scope.selectPage = function(page) {
////        if ( ! scope.isActive(page) ) {
////          scope.currentPage = page;
////          scope.onSelectPage({ page: page });
////        }
////      };

////      scope.selectPrevious = function() {
////        if ( !scope.noPrevious() ) {
////          scope.selectPage(scope.currentPage-1);
////        }
////      };
////      scope.selectNext = function() {
////        if ( !scope.noNext() ) {
////          scope.selectPage(scope.currentPage+1);
////        }
////      };
////    }
////  };
////});

angular
    .module("ui.bootstrap.pagination", [])
    .constant("paginationConfig", { boundaryLinks: !1, directionLinks: !0, firstText: "First", previousText: "Previous", nextText: "Next", lastText: "Last" })
    .directive("pagination", [
        "paginationConfig",
        function (t) {
            return {
                restrict: "EA",
                scope: { numPages: "=", currentPage: "=", maxSize: "=", onSelectPage: "&" },
                templateUrl: "template/pagination/pagination.html",
                replace: !0,
                link: function (e, n, o) {
                    function a(t, e, n, o) {
                        return { number: t, text: e, active: n, disabled: o };
                    }
                    var i = angular.isDefined(o.boundaryLinks) ? e.$eval(o.boundaryLinks) : t.boundaryLinks,
                        r = angular.isDefined(o.directionLinks) ? e.$eval(o.directionLinks) : t.directionLinks,
                        l = angular.isDefined(o.firstText) ? o.firstText : t.firstText,
                        s = angular.isDefined(o.previousText) ? o.previousText : t.previousText,
                        c = angular.isDefined(o.nextText) ? o.nextText : t.nextText,
                        u = angular.isDefined(o.lastText) ? o.lastText : t.lastText;
                    e.$watch("numPages + currentPage + maxSize", function () {
                        e.pages = [];
                        var t = 1,
                            n = e.numPages;
                        e.maxSize && e.maxSize < e.numPages && ((t = Math.max(e.currentPage - Math.floor(e.maxSize / 2), 1)), (n = t + e.maxSize - 1), n > e.numPages && ((n = e.numPages), (t = n - e.maxSize + 1)));
                        for (var o = t; n >= o; o++) {
                            var p = a(o, o, e.isActive(o), !1);
                            e.pages.push(p);
                        }
                        if (r) {
                            var d = a(e.currentPage - 1, s, !1, e.noPrevious());
                            e.pages.unshift(d);
                            var f = a(e.currentPage + 1, c, !1, e.noNext());
                            e.pages.push(f);
                        }
                        if (i) {
                            var g = a(1, l, !1, e.noPrevious());
                            e.pages.unshift(g);
                            var m = a(e.numPages, u, !1, e.noNext());
                            e.pages.push(m);
                        }
                        e.currentPage > e.numPages && e.selectPage(e.numPages);
                    }),
                        (e.noPrevious = function () {
                            return 1 === e.currentPage;
                        }),
                        (e.noNext = function () {
                            return e.currentPage === e.numPages;
                        }),
                        (e.isActive = function (t) {
                            return e.currentPage === t;
                        }),
                        (e.selectPage = function (t) {
                            !e.isActive(t) && t > 0 && e.numPages >= t && ((e.currentPage = t), e.onSelectPage({ page: t }));
                        });
                },
            };
        },
    ]);