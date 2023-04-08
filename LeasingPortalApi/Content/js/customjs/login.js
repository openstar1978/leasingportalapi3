app.controller('supplierregistercontroller', ['$scope', '$http', '$rootScope', '$window', '$sce', function ($scope, $http, $rootScope, $window, $sce) {
    $scope.MoreContacts = [{
        susername: '',
        sposition: '',
        semailaddress: '',
        sphone: ''
    }];
    $scope.addRow = function () {
        $scope.MoreContacts.push({
            susername: '',
            sposition: '',
            semailaddress: '',
            sphone: ''
        });
    }
    $scope.deleteRow = function (index) {
        $scope.MoreContacts.splice(index, 1);
    }
    var flag = false;
    var ermsg = "<strong>Error : </strong>";
    var date = new Date();
    var edate = date.getFullYear() + "-" + (("0" + (date.getMonth() + 1)).slice(-2)) + "-" + date.getDate()
    $scope.addressess = [];
    $scope.allAddress = [];
    $scope.getaddbtntxt = "Find the address";
    $scope.getaddressdisable = false;
    $scope.getAddress = function () {
        $scope.addressess = [];
        var postcode = jQuery("#reg_postcode").val();
        var apikey = "gpVL4Uf-OECpRvzOjZnKWA33077";
        $scope.getaddressdisable = true;
        $scope.getaddbtntxt = "Loading..";
        $http.get("https://api.getaddress.io/find/" + postcode + "?api-key=" + apikey + "&expand=true&sort=true").success(function (data, status) {
            if (data != null) {
                //jQuery("#reg_postcode").val(data["postcode"]);
                $scope.allAddress = data["addresses"];
                var i = 0;
                $scope.allAddress.forEach(function (obj) {
                    var newobj = {
                        Code: i,
                        Name: jQuery.grep(obj.formatted_address, Boolean).join(", ")
                    };
                    $scope.addressess.push(newobj);
                    i++;
                })

            }
            else {
                $scope.getaddbtntxt = "Find address";
            }
            $scope.getaddbtntxt = "Find the address";
            $scope.getaddressdisable = false;


        });
    }
    $scope.getSelectedAddress = function (id) {

        
        var d = $scope.allAddress[id.Code];
        console.log(d);
        var s = d["line_1"] + (d["line_2"] != "" ? ", " + d["line_2"] : "") + (d["line_3"] != "" ? ", " + d["line_3"] : "") + (d["line_4"] != "" ? ", " + d["line_4"] : "");
        jQuery("#reg_address1").val(s);
        //jQuery("#reg_address2").val();
        jQuery("#reg_city").val(d["town_or_city"]);
        jQuery("#reg_county").val(d["county"]);
    }
    jQuery("#datepicker").datepicker({
        autoclose: true,
        todayHighlight: true,
        format: 'dd/mm/yyyy'
    }).datepicker('update', new Date())
                .change('changeDate', function (ev) {
                    newDate = new Date(ev.date);
                    edate = newDate.getFullYear() + "-" + (("0" + (newDate.getMonth() + 1)).slice(-2)) + "-" + newDate.getDate();
                });
    jQuery("#simpleFelixform").validate({
        rules: {
            morgname: {
                required: true,
            },
            mname: {
                required: true,
            },
            museridemail: {
                required: true,
                email: true,
            },
            mpwd: {
                required: true,
                minlength: 6,
                pwcheck: true,
            },
            mtown: {
                required: true,

            },
            mpostcode:{
                required:true,
            },
            mcountry: {
                required:true,
            },
            maddress1:{
                required:true
            },
            mphone: {
                required: true
            },
            terms:{
                required:true
            },
        },
        messages: {
            mpwd: {
                pwcheck: "password must contain one uppercase,one lowercase and one digit",
            }
        },
        submitHandler: function (form) {
            var email = jQuery("#reg_email").val();

            jQuery.ajax({
                type: 'POST',
                url: '/Supplier/Registration/checkUser',
                data: { email: email },

                async: false,
                success: function (re) {
                    if (re == "Found") {
                        flag = false;
                        ermsg += "Looklike you already registered ! Try to login";
                    }
                    else {
                        flag = true;

                    }
                }
            });

        }
    });
    jQuery.validator.addMethod("pwcheck",
        function (value, element) {
            return /^[A-Za-z0-9\d=!\-@._*]*$/.test(value) // consists of only these
                && /[a-z]/.test(value) // has a lowercase letter
                && /\d/.test(value) // has a digit
        });
    jQuery("#simpleFelixform").on("submit", function (event) {
        event.preventDefault();
        var spinner = jQuery('#loader2');
        var $this = jQuery(this);

        var complogo = "";

        if (flag) {
            /*if (window.FormData !== undefined) {

                var fileData = new FormData();

                var documentsData = [];
                var d = 0;
                var obj = {};
                var fileUpload = jQuery('#leadFile').get(0);
                var files = fileUpload.files;
                if (files.length > 0) {
                    for (var i = 0; i < files.length; i++) {
                        fileData.append(files[i].name, files[i]);

                    }

                    jQuery.ajax({
                        url: '/Seller/Registration/uploadSupplierLogo',
                        type: "POST",
                        contentType: false, // Not to set any content header
                        processData: false, // Not to process data
                        data: fileData,
                        success: function (res) {
                            savedata(res);
                            
                        }
                    });

                }
                else {
                    savedata("");
                }
                //alert(complogo);
                
            }*/
            savedata("");
        }
        else {
            if (ermsg != "<strong>Error : </strong>" && ermsg != "") {
                jQuery(".woocommerce-notices-wrapper").find(".woocommerce-error li").html(ermsg);
                jQuery(".woocommerce-notices-wrapper").show();
                jQuery('html, body').animate({
                    scrollTop: jQuery(".woocommerce-notices-wrapper").offset().top - 10
                }, 1000);
            }
        }
    });

    function savedata(res) {

        var spinner = jQuery('#loader2');
        var tplaces = "";
        jQuery(".mtplaces").each(function (e) {
            if (jQuery(this).is(":checked")) {
                if (tplaces == "") {
                    tplaces = jQuery(this).val();
                } else {
                    tplaces =tplaces+","+jQuery(this).val();
                }
            }
        })
        var emp = $("#reg_mcat").val();
        var mcatlist = emp.toString();
        var SupplierViewModel = {
            "com_logo": res,
            "museridemail": jQuery("#reg_email").val(),
            "mpwd": jQuery("#mpwd").val(),
            "mname": jQuery("#reg_name").val(),
            "mlname": jQuery("#reg_lname").val(),
            "mphone": jQuery("#reg_phone").val(),
            "morgname": jQuery("#reg_org").val(),
            "morgregno": jQuery("#reg_regno").val(),
            "mpostcode": jQuery("#reg_postcode").val(),
            "maddress1": jQuery("#reg_address1").val(),
            "maddress2": jQuery("#reg_address2").val(),
            "mcity": jQuery("#reg_city").val(),
            "mcounty": jQuery("#reg_county").val(),
            "mcountry": jQuery("#reg_country option:selected").val(),
            "musertype": jQuery("#org_type").val(),
            "use_lease": jQuery("#org_lease").val(),
            "website": jQuery("#reg_website").val(),
            "source_enquiry": jQuery("#reg_enquiry").val(),
            "trading_as": jQuery("#reg_trading").val(),
            "date_established": edate,
            "target_places": tplaces,//jQuery("#reg_mtplaces").val(),
            "com_type": jQuery("#org_type").val(),
            "owner": jQuery("#reg_owner").val(),
            "com_email": jQuery("#reg_comemail").val(),
            "mposition": jQuery("#reg_mposition").val(),
            "mcatlist": mcatlist,
        };
        /*var suppliermoreviewmodel = {
            "monthly_sales": jQuery("#reg_msales").val(),
            "monthly_lease": jQuery("#reg_mlease").val(),
            "education": jQuery("#reg_education").val(),
            "commercial": jQuery("#reg_commercial").val(),
            "publicsector": jQuery("#reg_publicsector").val(),

        };
        var supplierequipmentviewmodel = {
            "equipment_sold": jQuery("#reg_equipment").val(),
            "equipment_brand": jQuery("#reg_ebrand").val(),
            "warrantly_length": jQuery("#reg_warranty").val(),
            "warranty_provider": jQuery("#org_wprovider").val(),

        };*/
        var suppliercontactsViewmodel = $scope.MoreContacts;
        var sizeDetailData = [];
        /*var frmValues = $this.serializeArray()
                            .filter(function (elem) {
                                return jQuery.trim(elem.value) != "";
                            });
       */
        var a = jQuery(this), l = jQuery(this).closest("form");
        jQuery.ajax({
            type: "post",
            url: "/Supplier/Registration/Register",
            //data: { "model": SupplierViewModel, "equipment": supplierequipmentviewmodel, "more": suppliermoreviewmodel, "contacts": suppliercontactsViewmodel },
            data: { "model": SupplierViewModel },
            beforeSend: function () {
                spinner.show();

            }, success: function (data) {

                if (data == "Success") {
                    spinner.hide();
                    jQuery(".customize-form").remove();
                    jQuery(".thankyou-form").show();
                    //jQuery('html').addClass('m-page--loading');
                    //jQuery('#loadtext').html('Synchronising your account..');
                    //$('.m-page-loader.m-page-loader--base').addClass('myclass');
                    // window.location.href = "/Dashboard";
                } else {
                    //setTimeout(function(){
                    spinner.hide();
                    if (ermsg != "<strong>Error : </strong>" && ermsg != "") {
                        ermsg += "Something went wrong! Try again."
                        jQuery(".woocommerce-notices-wrapper").find(".woocommerce-error li").html(ermsg);
                        jQuery(".woocommerce-notices-wrapper").show();
                        jQuery('html, body').animate({
                            scrollTop: jQuery(".woocommerce-notices-wrapper").offset().top - 10
                        }, 1000);
                    }

                }
            }
        }).done(function (resp) {
            spinner.hide();
            // alert(resp.status);
        });
    }
}]);
jQuery(function () {

});
