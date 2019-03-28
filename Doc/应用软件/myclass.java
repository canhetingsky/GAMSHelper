package TEST;

public class myclass {

	public static void main(String[] args) {
		String filePath = "C:\\Users\\Administrator\\Desktop\\GAMSHelper\\GAMSDemo\\GAMSDemo\\bin\\x64\\Release\\GAMSDemo.exe";
		String server = "DESKTOP-36C9L6T";
		String database = "lushushu";
		String user = "sa";
		String pwd = "123";
		Runtime rn = Runtime.getRuntime();
		Process p = null;
		try {
			String command = filePath+" "+server+" "+database+" "+user+" "+pwd;
			System.out.println("start");
			p = rn.exec(command);
			System.out.println("......");
			p.waitFor();
			System.out.println("OK");
		} catch (Exception e) {
			System.out.println("Error!");
		}
	}

}
