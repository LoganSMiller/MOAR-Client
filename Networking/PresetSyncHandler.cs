
using Fika.Client;
using Fika.Core.Networking;
using Fika.Core.Packets;

public class PresetSyncHandler
{
    public static void Register()
    {
        CoopHandler.Instance.Register<PresetSyncPacket>(ref packet => {
            // TODO: apply preset to local UI or memory
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
            if (JsonSchemaValidator.ValidatePreset(json))
                UnityEngine.Debug.Log($"[MOAR] Valid preset received: {{packet.PresetName}}");
        });
    }
}
