function addToCart(dishId) {
    $.ajax({
        url: '/Cart/AddToCart/',
        type: 'POST',
        data: { dishId: dishId },
        success: function () {
            updateCart();
        },
        error: function (error) {
            console.error('Ошибка при добавлении в корзину:', error);
        }
    });
}

function updateCart() {
    $.ajax({
        url: '/Cart/GetCart/',
        type: 'GET',
        data: { establishmentId: establishmentId },
        success: function (response) {
            $('#cart').html(response);
        },
        error: function (error) {
            console.error('Ошибка при обновлении корзины:', error);
        }
    });
}

function removeFromCart(cartId) {
    $.ajax({
        url: '/Cart/RemoveFromCart/',
        type: 'POST',
        data: { cartId: cartId },
        success: function (response) {
            updateCart();
        },
        error: function (error) {
            console.error('Ошибка при удалении из корзины:', error);
        }
    });
}

$(document).ready(function() {
    let establishmentId = '@Model.Id';
    updateCart(establishmentId);
});
     