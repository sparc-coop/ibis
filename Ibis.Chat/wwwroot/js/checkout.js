

const stripe = Stripe('pk_live_51LxZjWAtmjNehy1iYa5yZUzRcguXWhNqR5enIgHx5OhN0joI4IrIraYsrdVm97bENRsAJgHQUnYUP0pvjVifnqM100ro4jk7rP');
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