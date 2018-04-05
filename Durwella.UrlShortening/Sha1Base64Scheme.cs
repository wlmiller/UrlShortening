namespace Durwella.UrlShortening
{
    public class Sha1Base64Scheme :
        BaseHashScheme<Sha1HashFunction, Base64StringEncoder>
    {
        public Sha1Base64Scheme(IConfigSettings config)
        {
            LengthPreference = config?.PreferredHashLength ?? 4;
        }
    }
}