

const stripe = Stripe('pk_test_51LxZjWAtmjNehy1iYH6vbqIS9bucufNTvQWCyzQD9do9Wy2TyFv834Xj2LZurDEwcx37eZ8twMzh6scj3TMlNsb200WsSIYYcc');
let elements, clientsecret;

function initStripe(secret, id) {
    if (elements && clientsecret == secret)
        return;

    clientsecret = secret;

    const options = {
        clientSecret: secret,
        appearance: {/*...*/ },
    };

    // Set up Stripe.js and Elements to use in checkout form, passing the client secret obtained in step 2
    elements = stripe.elements(options);

    // Create and mount the Payment Element
    const paymentElement = elements.create('payment');
    paymentElement.mount(id);
}

async function confirmStripe(returnUrl) {
    const { result } = await stripe.confirmPayment({
        elements,
        confirmParams: {
            return_url: returnUrl
        },
        redirect: 'if_required'
    });

    return "";
}

async function getStatusStripe(secret) {
    return await stripe.retrievePaymentIntent(secret);
}