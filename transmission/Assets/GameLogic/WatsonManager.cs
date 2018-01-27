using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Services.NaturalLanguageUnderstanding.v1;
using IBM.Watson.DeveloperCloud.Connection;

public class WatsonManager : MonoBehaviour
{
    //Speech to Text
    private string _sttUsername = "928c9c5d-9e72-4456-b185-6fba85cb9ba4";
    private string _sttPassword = "F1ooTHTxGyVy";
    private string _sttURL = "https://stream.watsonplatform.net/speech-to-text/api";

    private string _nluUserName = "ec7a6666-9311-4ea2-bba0-af82da5d0846";
    private string _nluPassword = "3j2bmZwCOYNS";
    private string _nluURL = "https://gateway.watsonplatform.net/natural-language-understanding/api";

    public Text ResultsField;

    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

    private SpeechToText _speechToText;

    //NLU
    private NaturalLanguageUnderstanding _naturalLanguageUnderstanding;

    private bool _getModelsTested = false;
    private bool _analyzeTested = false;
    private string _nluAuthenticationToken;

    private WatsonResponse watsonResponse = new WatsonResponse();
    public MessageSender messageSender;
    void Start()
    {
        if (!Utility.GetToken(OnGetToken, _nluURL, _nluUserName, _nluPassword))
            Log.Debug("ExampleGetToken.Start()", "Failed to get token.");

        LogSystem.InstallDefaultReactors();

        //Speech to Text
        Credentials sttCredentials = new Credentials(_sttUsername, _sttPassword, _sttURL);

        _speechToText = new SpeechToText(sttCredentials);
        Active = true;
        StartRecording();

        //NLU
        Credentials nluCredentials = new Credentials(_nluUserName, _nluPassword, _nluURL)
        {
            AuthenticationToken = _nluAuthenticationToken
        };
        _naturalLanguageUnderstanding = new NaturalLanguageUnderstanding(nluCredentials);
        NLUAnalyze("I hate you so much! Analyze");
    }

    public void EnableWatsonSST()
    {
        StartRecording();
    }


    public void DisableWatsonSST()
    {
        StopRecording();
    }


    //NLU AuthenticationToken
    private void OnGetToken(AuthenticationToken authenticationToken, string customData)
    {
        _nluAuthenticationToken = authenticationToken.ToString();
        //Log.Debug("ExampleGetToken.OnGetToken()", "created: {0} | time to expiration: {1} minutes | token: {2}", _authenticationToken.Created, _authenticationToken.TimeUntilExpiration, _authenticationToken.Token);
    }

    //Setting up NLU Parameters
    private Parameters CreatingParametersNLU(string message) {
        Parameters parameters = new Parameters()
        {
            text = message,
            return_analyzed_text = true,
            language = "en",
            features = new Features()
            {
                sentiment = new SentimentOptions()
                {
                    document = true,
                    targets = new string[] { "Analyze" }
                },
                emotion = new EmotionOptions()
                {
                    document = true,
                    targets = new string[] { "Analyze" }
                }
            }
        };
        return parameters;
    }

    private void OnGetModels(ListModelsResults resp, Dictionary<string, object> customData)
    {
        Log.Debug("ExampleNaturalLanguageUnderstanding.OnGetModels()", "ListModelsResult: {0}", customData["json"].ToString());
        _getModelsTested = true;
    }

    private void OnAnalyze(AnalysisResults resp, Dictionary<string, object> customData)
    {
        Log.Debug("ExampleNaturalLanguageUnderstanding.OnAnalyze()", "AnalysisResults: {0}", customData["json"].ToString());
        string label = "";
        if(resp.sentiment.document.score < 0 )
        {
            label = "negative";
        }
        else
        {
            label = "postive";
        }
        Debug.Log("This is your sentiemnt scores:  " + resp.sentiment.document.score + " label: " + label);
        float sadness = resp.emotion.document.emotion.sadness;
        float joy = resp.emotion.document.emotion.joy;
        float fear = resp.emotion.document.emotion.fear;
        float disgust = resp.emotion.document.emotion.disgust;
        float anger = resp.emotion.document.emotion.anger;
        Debug.Log("This is your sadness: " + sadness+ ", joy: " + joy + " fear: " + fear + " disgust: " + disgust + " anger: " + anger);

        watsonResponse.sentiementScore = resp.sentiment.document.score;
        watsonResponse.sentiementLabel = label;
        watsonResponse.sadnessScore = sadness;
        watsonResponse.joyScore = joy;
        watsonResponse.fearScore = fear;
        watsonResponse.disgustScore = disgust;
        watsonResponse.angerScore = anger;
        messageSender.SendResponse(watsonResponse);
        _analyzeTested = true;
    }

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleNaturalLanguageUnderstanding.OnFail()", "Error received: {0}", error.ToString());
    }

    private void NLUAnalyze(string message) {
        Parameters parameters = CreatingParametersNLU(message);
        if (!_naturalLanguageUnderstanding.Analyze(OnAnalyze, OnFail, parameters))
            Log.Debug("ExampleNaturalLanguageUnderstanding.Analyze()", "Failed to get models.");
        Log.Debug("ExampleNaturalLanguageUnderstanding.Examples()", "Natural language understanding examples complete.");

    }

    //Speech to Text
    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio,
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    //Log.Debug("ExampleStreaming.OnRecognize()", text);
                    //ResultsField.text = text;
                    if(res.final)
                    {
                        if(text.Split().Length > 3)
                        {
                            var usermessage = text.Substring(0, text.Length - "(Final, 0.99)".Length - 2);
                            // watsonResponse.userMessage = userMessage;
                            usermessage += " Analyze";
                            Log.Debug("This is your message", usermessage);
                            NLUAnalyze(usermessage);
                        }

                    }
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }
}
