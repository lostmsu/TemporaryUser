namespace TemporaryUser;

public class TemporaryUserTests {
    [Fact]
    public void AddsAndDeletes() {
        string name = nameof(this.AddsAndDeletes);
        var user = new TemporaryUser(name: name, password: name);
        user.Dispose();
    }

    [Fact]
    public void ProfilePath() {
        string name = nameof(this.ProfilePath);
        using var user = new TemporaryUser(name: name, password: name);
        Assert.Equal(name, user.ProfilePath.Name);
    }
    
    [Fact]
    public void DoubleDispose() {
        string name = nameof(this.DoubleDispose);
        using var user = new TemporaryUser(name: name, password: name);
        // ReSharper disable once DisposeOnUsingVariable - testing correct behavior
        user.Dispose();
    }
}