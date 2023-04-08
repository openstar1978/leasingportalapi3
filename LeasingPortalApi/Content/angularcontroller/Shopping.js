app.controller('ShoppingCartController', ['$scope', '$http', '$rootScope', '$location', '$sce', '$cookies', function ($scope, $http, $rootScope, $location, $sce, $cookies) {
    var expireDate = new Date();
    expireDate.setDate(expireDate.getDate() + 365);
    $scope.isCheckoutPage = false;
    $rootScope.shoppingCart = $rootScope.shoppingCart || [];
    $rootScope.shoppingCart.totalPrice = 0;
  
        
    if (!angular.isUndefined($cookies.get('leasebasket'))) {
        var id = $cookies.getObject('leasebasket');
            $http.get('/api/CookieApi/GetCookieCart?id=' + id).success(function (data) {
                if (data != "empty") {
        
                $rootScope.shoppingCart = data.data;
                $rootScope.shoppingCart.totalPrice = 0;
                $rootScope.shoppingCart.totalItems = 0;
                //$scope.getTotalPrice();
                $scope.getTotalNoOfItems();
                $scope.loader = false;
                } else {
                    $cookies.remove("leasebasket", { path: '/' });
                    $rootScope.shoppingCart.totalPrice = 0;
                    $rootScope.shoppingCart.totalItems = 0;
                    $scope.getTotalNoOfItems();
                    $scope.loader = false;
                }
            });
        
        
    }
    $rootScope.$on('itemList', function (event, itemList) {
        $scope.itemList = itemList;
    });
    
    $scope.$watch('shoppingCart', function () {
        $rootScope.shoppingCart.totalPrice = 0;
        $rootScope.shoppingCart.totalItems = 0;
        //$scope.getTotalPrice();
        if ($scope.shoppingCart.length > 0) {
            $scope.getTotalNoOfItems();

        }
    }, true);

    $scope.checkout = function () {
        
        $location.path('/checkout');
        $scope.isCheckoutPage = true;

    };

    $scope.deleteFromCart = function (index) {
        var id = $cookies.getObject('leasebasket');
        $http.get('/api/CookieApi/GetCartDelete?id=' + $rootScope.shoppingCart[index].Id).success(function (data) {
            if (data === "delete") {
                $rootScope.shoppingCart.splice(index, 1);
                if ($rootScope.shoppingCart.length <= 0) {
                    $cookies.remove("leasebasket", { path: '/' });

                }
                else {
                    $cookies.putObject('leasebasket', id, { 'expires': expireDate, 'path': '/' });
                }
                toastr.success("Product remove from Your Basket", "Success");
            }

        });

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
            $rootScope.shoppingCart.totalPrice = Number($rootScope.shoppingCart.totalPrice) + $scope.getItemPrice(itemObj);
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
        itemPrice = (Number(itemObj.Product.mprice) * Number(itemObj.Quantity)).toFixed(2);
        return Number(itemPrice);
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