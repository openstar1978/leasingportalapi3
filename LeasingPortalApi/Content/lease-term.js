(function (angular, factory) {
    if (typeof define === 'function' && define.amd) {
        define('lease-term', ['angular'], function (angular) {
            return factory(angular);
        });
    } else {
        return factory(angular);
    }
}(typeof angular === 'undefined' ? null : angular, function (angular) {
    var commonModule = angular.module('leasetermjs', []);
    'use strict';
    commonModule.factory('CommonLeaseTerm', ['$rootScope', '$http', '$window', '$compile',
        function ($rootScope, $http, $window, $compile) {
            var self = this;



            var randomObject = {};
            var dprice = {};
            var number = Math.floor(Math.random() * 100);
            randomObject.updateTerms = function (basicprice, freq, period) {
                basicprice = Number(basicprice);
                try {
                    if (basicprice > 0 && basicprice <= 9999) {
                        switch (freq) {
                            case 1:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.09112)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.04962)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.03469)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.02797)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.024)).toFixed(2);
                                        break;
                                }
                                break;
                            case 3:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.26674)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.14282)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.10245)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.08252)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.07075)).toFixed(2);
                                        break;
                                }
                                break;
                            case 4:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.35506)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.19185)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.13555)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.10913)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.09353)).toFixed(2);
                                        break;
                                }
                                break;
                            case 6:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.52381)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.28679)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.20029)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.16576)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.14295)).toFixed(2);
                                        break;
                                }
                                break;
                            case 12:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 1)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.54545)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.38384)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.30808)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.26328)).toFixed(2);
                                        break;
                                }
                                break;
                        }
                    }
                    else if (basicprice >= 10000 && basicprice <= 19999) {
                        switch (freq) {
                            case 1:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.09033)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.04919)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.03378)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.02698)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.02299)).toFixed(2);
                                        break;
                                }
                                break;
                            case 3:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.26582)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.14056)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.09998)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.07989)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.06798)).toFixed(2);
                                        break;
                                }
                                break;
                            case 4:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.35293)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.18904)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.13242)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.10577)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.08998)).toFixed(2);
                                        break;
                                }
                                break;
                            case 6:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.52153)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.27956)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.19607)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.16112)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.13799)).toFixed(2);
                                        break;
                                }
                                break;
                            case 12:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 1)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.53271)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.37783)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.30106)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.25551)).toFixed(2);
                                        break;
                                }
                                break;
                        }
                    }
                    else if (basicprice >= 20000) {
                        switch (freq) {
                            case 1:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.08954)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.04832)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.03289)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.02607)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.02202)).toFixed(2);
                                        break;
                                }
                                break;
                            case 3:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.26397)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.13831)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.09754)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.07729)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.06526)).toFixed(2);
                                        break;
                                }
                                break;
                            case 4:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.34972)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.18623)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.12933)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.10246)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.08649)).toFixed(2);
                                        break;
                                }
                                break;
                            case 6:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 0.51691)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.27226)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.19185)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.15192)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.12812)).toFixed(2);
                                        break;
                                }
                                break;
                            case 12:
                                switch (period) {
                                    case 12:
                                        return (Number(basicprice * 1)).toFixed(2);
                                        break;
                                    case 24:
                                        return (Number(basicprice * 0.5283)).toFixed(2);
                                        break;
                                    case 36:
                                        return (Number(basicprice * 0.37174)).toFixed(2);
                                        break;
                                    case 48:
                                        return (Number(basicprice * 0.29396)).toFixed(2);
                                        break;
                                    case 60:
                                        return (Number(basicprice * 0.24796)).toFixed(2);
                                        break;
                                }
                                break;
                        }
                    }
                } catch (e) {
                    alert("There is error which shows " + e.message);
                }
                return randomObject;
                //return number; 
            };
            randomObject.updatedprice = function (aprice, mprice, freq, period) {
                mprice = Number(mprice);
                aprice = Number(aprice);
                try {
                    var atprice = aprice * (period / 12);
                    tprice = mprice * (12 / freq) * (period / 12);
                    return Number(tprice - atprice).toFixed(2);
                } catch (e) {
                    alert("There is error which shows " + e.message);
                }
                return randomObject;
                //return number; 
            };
            randomObject.priceFilter = function (basicprice) {
                basicprice = Number(basicprice);
                try {
                    if (basicprice >= 0 && basicprice < 240) {
                        return (Number(basicprice / 0.024)).toFixed(2);
                    } else if (basicprice >= 240 && basicprice <= 459.77) {
                        return (Number(basicprice / 0.02299)).toFixed(2);
                    } else {
                        return (Number(basicprice / 0.02202)).toFixed(2);
                    }
                } catch (e) {
                    alert("There is error which shows " + e.message);
                }
                return randomObject;
                //return number; 
            };
            return randomObject;

        }]);
}));