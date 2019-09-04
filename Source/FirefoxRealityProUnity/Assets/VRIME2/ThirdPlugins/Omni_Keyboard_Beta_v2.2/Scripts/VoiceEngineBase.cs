namespace Htc.Omni
{
    public abstract class VoiceEngineBase
    {
        public abstract string GetVoiceStringByFile(string filePath, out string error, SupportLanguage language , out bool httpError);
        public abstract string VoiceEngineName();
    }

}