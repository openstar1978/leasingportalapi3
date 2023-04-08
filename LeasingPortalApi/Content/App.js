(function(){


    var app = angular.module('MyApp', ['ngSanitize', 'ngRoute', 'ngCookies'])
    app.factory('ShoppingFactory', ['AccountsService', 'ItemsService', '$rootscope', function (AccountsService, ItemsService, $rootscope) {
        return {
            createCurrentUser: function () {
                this.username = '';
                this.password = '';
                this.shoppingcart = [];
                $rootscope.currentUser = this;
            },
            createShoppingCart: function () {
                $rootscope.shoppingcart = [];

                $rootscope.min = 0;
                $rootscope.max = 1000;
            }
        };
    }]);
    app.config(function ($locationProvider) {
        //$locationProvider.html5Mode(true); 
    });
    console.log('changed');
    app.directive('ngLoading', function (Session, $compile) {

        var loadingSpinner = '<div class="spinner"><div class="double-bounce1"></div><div class="double-bounce2"></div></div>';

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                var originalContent = element.html();
                element.html(loadingSpinner);
                scope.$watch(attrs.ngLoading, function (val) {
                    if (val) {
                        element.html(originalContent);
                        $compile(element.contents())(scope);
                    } else {
                        element.html(loadingSpinner);
                    }
                });
            }
        };
    });
    app.controller('MenuController', ['$scope', '$http',
    function ($scope, $http) {
        $scope.SiteMenu = [];
        $http.get('/api/HomeApi/GelAllMainCategories').success(function (data) {
            $scope.SiteMenu = data;
        });
    }
    ]);

    /*****************************Shopping Controller******************************************/
    app.controller('ShoppingCartController', ['$scope', '$http', '$rootScope', '$location', '$sce', '$cookies', function ($scope, $http, $rootScope, $location, $sce, $cookies) {
        $scope.isCheckoutPage = false;
        $rootScope.shoppingCart = $rootScope.shoppingCart || [];
        $rootScope.shoppingCart.totalPrice = 0;
        if (!angular.isUndefined($cookies.get('leasebasket'))) {
            $rootScope.shoppingCart = $cookies.getObject('leasebasket');
        }
        /*$http.get('/Product/GetCart').success( function (data) {
            $rootScope.shoppingCart = $cookies.getObject('leasebasket');;
            
            console.log($scope.shoppingCart);
            $scope.loader = false;
        });
        */
        $rootScope.$on('itemList', function (event, itemList) {
            $scope.itemList = itemList;
        });

        $scope.$watch('shoppingCart', function () {
            $rootScope.shoppingCart.totalPrice = 0;
            $rootScope.shoppingCart.totalItems = 0;
            $scope.getTotalPrice();
            $scope.getTotalNoOfItems();
            console.log('changed');
        }, true);

        $scope.checkout = function () {
            $location.path('/checkout');
            $scope.isCheckoutPage = true;
        };

        $scope.deleteFromCart = function (index) {
            $rootScope.shoppingCart.splice(index, 1);
        };

        $scope.clearShoppingCart = function (index) {
            $rootScope.shoppingCart = [];
        };

        $scope.backToList = function () {
            $location.path('/list');
            $scope.isCheckoutPage = false;
        };

        $scope.getTotalPrice = function () {
            $rootScope.shoppingCart.forEach(function (itemObj) {
                $rootScope.shoppingCart.totalPrice += $scope.getItemPrice(itemObj);
            });
        };

        $scope.getTotalNoOfItems = function () {
            $rootScope.shoppingCart.forEach(function (itemObj) {
                $rootScope.shoppingCart.totalItems += $scope.getItemNo(itemObj);
            });
        };

        $scope.getItemPrice = function (itemObj) {
            var itemPrice = 0;
            console.log("items");
            itemPrice = Number(itemObj.Product.mprice) * Number(itemObj.Quantity);
            return itemPrice;
        };

        $scope.getItemNo = function (itemObj) {
            return itemObj.Quantity;
        };

        $scope.confirmCheckout = function () {
            $location.path('/checkoutsuccess');
            $scope.message = $sce.trustAsHtml($rootScope.shoppingCart.length + ' Items' + ' @@ $' + $rootScope.shoppingCart.totalPrice);
            console.log($scope.successMsg);
            $rootScope.shoppingCart = [];
        };
    }]);
})();