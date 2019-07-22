package TEST;

public class Main {

	public static void main(String[] args) {
		String filePath = "C:\\Users\\Administrator\\Desktop\\GAMSHelper\\GAMSDemo\\GAMSDemo\\bin\\x64\\Release\\GAMSDemo.exe";
		String server = "DESKTOP-36C9L6T";
		String database = "lushushu";
		String user = "sa";
		String pwd = "123";
		String time= "8:30";
		String debug = "NONE";
		Runtime rn = Runtime.getRuntime();
		Process p = null;
		try {
			String command = filePath+" "+server+" "+database+" "+user+" "+pwd+" "+time+" "+debug;
			System.out.println("start");
			System.out.println("command:  "+command);
			p = rn.exec(command);
			System.out.println("......");
			p.waitFor();
			System.out.println("OK");
		} catch (Exception e) {
			System.out.println("Error!");
		}
	}

}
