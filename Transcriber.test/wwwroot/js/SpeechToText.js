const speechConfig = SpeechConfig.fromSubscription("083676fb58824ce28526776557d95aae", "eastus");


const audioConfig = AudioConfig.fromDefaultMicrophoneInput();
const recognizer = new SpeechRecognizer(speechConfig, audioConfig);

recognizer.recognizeOnceAsync(result => {
    // Interact with result
    switch (result.reason) {
        case ResultReason.RecognizedSpeech:
            console.log(`RECOGNIZED: Text=${result.text}`);
            console.log("    Intent not recognized.");
            break;
        case ResultReason.NoMatch:
            console.log("NOMATCH: Speech could not be recognized.");
            break;
        case ResultReason.Canceled:
            const cancellation = CancellationDetails.fromResult(result);
            console.log(`CANCELED: Reason=${cancellation.reason}`);

            if (cancellation.reason == CancellationReason.Error) {
                console.log(`CANCELED: ErrorCode=${cancellation.ErrorCode}`);
                console.log(`CANCELED: ErrorDetails=${cancellation.errorDetails}`);
                console.log("CANCELED: Did you update the subscription info?");
            }
            break;
    }
});