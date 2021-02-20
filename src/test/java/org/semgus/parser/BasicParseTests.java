package org.semgus.parser;

import org.antlr.v4.runtime.CharStreams;
import org.antlr.v4.runtime.CommonTokenStream;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.MethodSource;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.PrintStream;
import java.nio.file.FileSystems;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.PathMatcher;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotEquals;

/**
 * Tests that valid examples parse and invalid examples don't parse
 */
public class BasicParseTests {

    /**
     * Root path to look for example files for the tests
     */
    private static Path resourceRoot = Path.of("src", "test", "resources");

    /**
     * Path matcher that is configured to match Semgus specification files (*.sem)
     */
    private static PathMatcher semMatcher = FileSystems.getDefault().getPathMatcher("glob:**.sem");


    /**
     * Tests that all Semgus files that should be valid are properly reported as being valid.
     * Note that we only print the parser errors to standard error if the test fails.
     * @param path File to check
     * @throws IOException if there is a problem reading files or directories during the test
     */
    @ParameterizedTest
    @MethodSource("getValidExamples")
    public void testValidExamples(Path path) throws IOException {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        PrintStream oldErr = System.err;
        System.setErr(new PrintStream(baos));
        try {
            SemgusLexer lexer = new SemgusLexer(CharStreams.fromPath(path));
            SemgusParser parser = new SemgusParser(new CommonTokenStream(lexer));
            SemgusParser.StartContext ctx = parser.start();
            assertEquals(0, parser.getNumberOfSyntaxErrors());
        } catch (Exception e) {
            baos.writeTo(oldErr);
            throw e;
        } finally {
            System.setErr(oldErr);
        }
    }

    /**
     * Tests that all Semgus files that should be invalid are properly reported as being invalid.
     * Note that we only print the parser errors to standard error if the test fails.
     * @param path File to check
     * @throws IOException if there is a problem reading files or directories during the test
     */
    @ParameterizedTest
    @MethodSource("getInvalidExamples")
    public void testInvalidExamples(Path path) throws IOException {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        PrintStream oldErr = System.err;
        System.setErr(new PrintStream(baos));
        try {
            SemgusLexer lexer = new SemgusLexer(CharStreams.fromPath(path));
            SemgusParser parser = new SemgusParser(new CommonTokenStream(lexer));
            SemgusParser.StartContext ctx = parser.start();
            assertNotEquals(0, parser.getNumberOfSyntaxErrors());
        } catch (Exception e) {
            baos.writeTo(oldErr);
            throw e;
        } finally {
            System.setErr(oldErr);
        }
    }

    /**
     * Gets a stream of Semgus specifications that are syntactically valid
     * @return Stream of Semgus files
     * @throws IOException if there is an error reading the example directory
     */
    public static Stream<Path> getValidExamples() throws IOException {
        return getSemFiles(Path.of("examples","valid"));
    }

    /**
     * Gets a stream of Semgus specifications that are syntactically invalid
     * @return Stream of Semgus files
     * @throws IOException if there is an error reading the example directory
     */
    public static Stream<Path> getInvalidExamples() throws IOException {
        return getSemFiles(Path.of("examples", "invalid"));
    }

    /**
     * Returns a stream of Semgus specification files in the given resource directory
     * @param relative Directory to search in, relative to src/test/resources
     * @return A stream of Semgus specification files
     * @throws IOException if there is an error reading the given relative directory
     */
    public static Stream<Path> getSemFiles(Path relative) throws IOException {
        return Files.walk(resourceRoot.resolve(relative))
                .filter(Files::isReadable)
                .filter(p -> semMatcher.matches(p));
    }


}
