using UnityEngine;


namespace RacistExperiment
{
    public class TemporalSound : ManagedMonoBehaviour
    {
        public AudioSource AudioSource;

        public AudioClip Start;
        public AudioClip GotAnswer;
        public AudioClip AnswerIsRight;
        public AudioClip AnswerIsWrong;
        public AudioClip AnswerIsLate;

        public void PlaySound_Start() => AudioSource.PlayOneShot(Start);
        public void PlaySound_GotAnswer() => AudioSource.PlayOneShot(GotAnswer);
        public void PlaySound_AnswerIsRight() => AudioSource.PlayOneShot(AnswerIsRight);
        public void PlaySound_AnswerIsWrong() => AudioSource.PlayOneShot(AnswerIsWrong);
        public void PlaySound_AnswerIsLate() => AudioSource.PlayOneShot(AnswerIsLate);
    }
}